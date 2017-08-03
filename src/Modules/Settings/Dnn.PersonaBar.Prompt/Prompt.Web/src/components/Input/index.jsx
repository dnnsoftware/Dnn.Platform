import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import GridCell from "dnn-grid-cell";
import { connect } from "react-redux";
import Localization from "localization";
import util from "../../utils";
import "../Prompt.less";
import {
    prompt as PromptActions
} from "../../actions";
import Cookies from 'universal-cookie';
const cookies = new Cookies();

class Input extends Component {
    constructor() {
        super();
    }
    componentDidUpdate() {
        const consoleHeight = cookies.get("dnn-prompt-console-height");
        if (consoleHeight) {
            this.configConsole(['config', consoleHeight]);
        }
    }
    setValue(value) {
        this.refs.cmdPromptInput.value = value;
    }
    getValue() {
        return this.refs.cmdPromptInput.value;
    }
    setFocus(focus) {
        if (focus)
            this.refs.cmdPromptInput.focus();
        else
            this.refs.cmdPromptInput.blur();
    }
    toggleInput(show) {
        this.refs.cmdPromptInputDiv.style.display = show ? "block" : "none";
    }
    getTabId() {
        const dnnVariable = JSON.parse(window.parent.document.getElementsByName("__dnnVariable")[0].value);
        return dnnVariable.sf_tabId;
    }
    runCmd() {
        let self = this;
        let { props } = self;
        let txt = self.getValue().trim();
        if (txt === "" && props.nextPageCommand !== null && props.nextPageCommand != "")
            txt = props.nextPageCommand;
        if (!self.tabId) {
            self.tabId = self.getTabId();
        }

        self.cmdOffset = 0; // reset history index
        self.setValue(""); // clearn input for future commands.

        if (txt === "") {
            return;
        } // don't process if cmd is emtpy
        props.dispatch(PromptActions.runLocalCommand("INFO", "\n" + txt + "\n", "cmd"));
        if (props.nextPageCommand === null || props.nextPageCommand === "")
            props.pushHistory(txt); // Add cmd to history
        // Client Command
        const tokens = txt.split(" "),
            cmd = tokens[0].toUpperCase();

        if (cmd === "CLS" || cmd === "CLEAR-SCREEN" || cmd === "EXIT" || cmd === "RELOAD") {
            props.dispatch(PromptActions.runLocalCommand(cmd, null));
            return;
        }
        if (cmd === "CLH" || cmd === "CLEAR-HISTORY") {
            self.history = [];
            sessionStorage.removeItem('dnn-prompt-console-history');
            props.dispatch(PromptActions.runLocalCommand(cmd, Localization.get("SessionHisotryCleared")));
            return;
        }
        if (cmd === "CONFIG") {
            this.configConsole(tokens);
            return;
        }

        if (cmd === "SET-MODE") {
            self.changeUserMode(tokens);
            return;
        }
        // Server Command
        props.busy(true);
        if (cmd === "HELP") {
            props.dispatch(PromptActions.runHelpCommand({ cmdLine: txt, currentPage: self.tabId }, () => {
                props.busy(false);
                self.setFocus(true);
            }, (error) => {
                props.dispatch(PromptActions.runLocalCommand("ERROR", error.responseJSON.Message));
                props.busy(false);
                self.setFocus(true);
            }));
        } else {
            props.dispatch(PromptActions.runCommand({ cmdLine: txt, currentPage: self.tabId }, () => { }, (error) => {
                props.dispatch(PromptActions.runLocalCommand("ERROR", error.responseJSON.Message));
                props.busy(false);
                self.setFocus(true);
            }));
        }
        this.setFocus(false);
    }


    changeUserMode(tokens) {
        let { props } = this;
        if (!tokens && tokens.length >= 2) {
            return;
        }
        let mode = null;
        if (this.hasFlag("--mode")) {
            mode = this.getFlag("--mode", tokens);
        } else if (!this.isFlag(tokens[1])) {
            mode = tokens[1];
        }
        if (mode) {
            props.dispatch(PromptActions.changeUserMode({ UserMode: mode.toUpperCase() }, () => {
                if (mode.toUpperCase() === "EDIT")
                    util.utilities.closePersonaBar();
                window.parent.document.location.reload(true);
            }, (error) => {
                this.setValue(error);
            }));
        }
    }
    isFlag(token) {
        return (token && token.startsWith('--'));
    }
    getFlag(flag, tokens) {
        let token = null;
        if (!tokens || tokens.length) {
            return null;
        }
        for (let i = 1; i < tokens.length; i++) {
            token = tokens[i];
            // did we find the flag name?
            if (this.isFlag(token) && (token.toUpperCase() === flag.toUpperCase())) {
                // is there a value to be had?
                if ((i + 1) < tokens.length) {
                    if (!this.isFlag(tokens[i + 1])) {
                        return tokens[i + 1];
                    } else {
                        // next token is a flag and not a value. return nothing.
                        return null;
                    }
                } else {
                    // found but no value
                    return null;
                }
            }
        }
        // not found
        return null;
    }
    configConsole(tokens) {
        let { props } = this;
        let height = null;
        if (this.hasFlag("--height")) {
            height = this.getFlag("--height", tokens);
        } else if (!this.isFlag(tokens[1])) {
            height = tokens[1];
        }

        if (height) {
            props.setHeight(height);
            cookies.set("dnn-prompt-console-height", height, { path: '/' });
        }
    }
    hasFlag(flag, tokens) {
        if (!tokens || tokens.length) return false;
        for (let i = 0; i < tokens.length; i++) {
            if (tokens[i].toUpperCase === flag.toUpperCase()) {
                return true;
            }
        }
        return false;
    }
    render() {
        return (
            <div className="dnn-prompt-input-wrapper" ref="cmdPromptInputDiv">
                <input className="dnn-prompt-input" ref="cmdPromptInput" />
            </div>
        );
    }
}
Input.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    nextPageCommand: PropTypes.string,
    pushHistory: PropTypes.func.isRequired,
    busy: PropTypes.func.isRequired,
    setHeight: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        nextPageCommand: state.prompt.nextPageCommand
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(Input);