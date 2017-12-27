import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization/Localization";
import { util, formatString } from "utils/helpers";
import "./Prompt.less";
import Cookies from "universal-cookie";
const cookies = new Cookies();
import {commands as Cmd, modes as Mode} from "constants/promptLabel";

class Input extends Component {

    componentDidUpdate() {
        //This should be replaced by User Personalization API
        const consoleHeight = cookies.get("dnn-prompt-console-height");
        if (consoleHeight) {
            this.configConsole(["config", consoleHeight]);
        }
    }

    setValue(value) {
        this.cmdPromptInput.value = value;
    }

    getValue() {
        return this.cmdPromptInput.value;
    }

    setFocus(focus) {
        focus ? this.cmdPromptInput.focus() : this.cmdPromptInput.blur();
    }

    getTabId() {
        const dnnVariable = JSON.parse(window.parent.document.getElementsByName("__dnnVariable")[0].value);
        return dnnVariable.sf_tabId;
    }

    readInput() {
        const { props } = this;
        let txt = this.getValue().trim();
        if (txt === "" && props.nextPageCommand !== null && props.nextPageCommand !== "") {
            txt = props.nextPageCommand;
        }
        props.updateHistory(txt);
        return txt;
    }

    runLocalCmd(txt) {
        const { props } = this;
        const { actions } = props;

        if (!this.tabId) {
            this.tabId = this.getTabId();
        }

        this.setValue(""); // clear input for future commands.

        // Client Command
        const tokens = txt.split(" ");
        const cmd = tokens[0].toUpperCase();

        let done = true;
        switch (cmd) {
            case Cmd.CLS:
            case Cmd.CLEAR_SCREEN:
            case Cmd.EXIT:
            case Cmd.RELOAD:
                actions.runLocalCommand(cmd, null);
                break;
            case Cmd.CLH:
            case Cmd.CLEAR_HISTORY:
                props.updateHistory("", true);
                actions.runLocalCommand(cmd, Localization.get("SessionHistoryCleared"));
                break;
            case Cmd.CONFIG:
                this.configConsole(tokens);
                break;
            case Cmd.SET_MODE:
                this.changeUserMode(tokens);
                break;
            default:
                done = false;
                break;
        }
        return done;
    }

    runServerCmd(txt) {
        const { props } = this;
        const { actions } = props;

        if (!this.tabId) {
            this.tabId = this.getTabId();
        }

        this.setValue(""); // clear input for future commands.

        // Client Command
        const tokens = txt.split(" ");
        const cmd = tokens[0].toUpperCase();

        // Server Command
        const errorCallback = (error) => {
            props.busy(false);
            actions.runLocalCommand("ERROR", error.responseJSON.Message);
        };

        props.busy(true);
        if (cmd === Cmd.HELP) {
            if (txt.toUpperCase() === "HELP") {
                actions.getCommandList({ cmdLine: "list-commands", currentPage: self.tabId }, () => {
                    props.busy(false);
                }, errorCallback.bind(this));
            } else {
                actions.runHelpCommand({ cmdLine: txt, currentPage: self.tabId }, () => {
                    props.busy(false);
                }, errorCallback.bind(this));
            }
        } else {
            if (!Cmd[cmd]) {
                actions.runCommand(
                    {cmdLine: txt, currentPage: self.tabId},
                    () => {
                        props.busy(false);
                    }, errorCallback.bind(this));
            }
        }
    }

    displayCmdInfo(txt) {
        const { props } = this;
        const { actions } = props;
        actions.runLocalCommand("INFO", "\n" + txt + "\n", "cmd");
    }

    runCmd() {
        const txt = this.readInput();
        if (txt) {
            this.displayCmdInfo(txt);
            if (!this.runLocalCmd(txt)) {
                this.runServerCmd(txt);
            }
            this.setFocus(false);
        }
    }

    changeUserMode(tokens) {
        const { props } = this;
        const { actions } = props;
        if (!tokens && tokens.length >= 2) { return; }

        tokens = tokens.map(token => token.toUpperCase());

        const [mode] = tokens.slice(-1);

        if (mode && !this.isFlag(mode) && Mode[mode]) {
            actions.changeUserMode({ UserMode: mode }, () => {
                if (mode === "EDIT")
                    util.utilities.closePersonaBar();
                window.parent.document.location.reload(true);
            }, (error) => actions.runLocalCommand("ERROR", error));
        } else {
            actions.runLocalCommand("ERROR", formatString(Localization.get("Prompt_FlagIsRequired"), "Mode"));
        }
    }

    isFlag(token) {
        return (token && token.startsWith("--"));
    }

    getFlag(flag, tokens) {
        if (!tokens || tokens.length === 0) { return null; }
        return tokens.find((token) => this.isFlag(token) && flag.toUpperCase() === token.toUpperCase());
    }

    configConsole(tokens) {
        const { props } = this;
        const { actions } = props;
        let height = null;
        if (this.hasFlag("--height")) {
            height = this.getFlag("--height", tokens);
        } else if (!this.isFlag(tokens[1])) {
            height = tokens[1];
        }

        if (height) {
            props.setHeight(height);
            cookies.set("dnn-prompt-console-height", height, { path: "/" });
        } else {
            actions.runLocalCommand("ERROR", formatString(Localization.get("Prompt_FlagIsRequired"), "Height"));
        }
    }

    hasFlag(flag, tokens) {
        return tokens ? tokens.find((token) => token.toUpperCase === flag.toUpperCase()) : false;
    }

    render() {
        return (
            <div className="dnn-prompt-input-wrapper">
                <input className="dnn-prompt-input" ref={(el) => this.cmdPromptInput = el} />
            </div>
        );
    }
}

Input.PropTypes = {
    nextPageCommand: PropTypes.string,
    updateHistory: PropTypes.func.isRequired,
    busy: PropTypes.func.isRequired,
    setHeight: PropTypes.func.isRequired,
    actions: PropTypes.object.isRequired
};

export default Input;