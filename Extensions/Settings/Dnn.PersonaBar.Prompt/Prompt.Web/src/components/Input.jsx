import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization/Localization";
import { util, formatString } from "utils/helpers";
import "components/Prompt.less";
//import Cookies from "universal-cookie";
//const cookies = new Cookies();
import {commands as Cmd, modes as Mode} from "constants/promptLabel";

class Input extends Component {

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
            actions.runLocalCommand("ERROR", error.responseJSON.Message);
        };

        if (cmd === Cmd.HELP) {
            if (txt.toUpperCase() === "HELP") {
                actions.getCommandList({ cmdLine: "list-commands", currentPage: self.tabId }, () => {
                }, errorCallback.bind(this));
            } else {
                actions.runHelpCommand({ cmdLine: txt, currentPage: self.tabId }, () => {
                }, errorCallback.bind(this));
            }
        } else {
            if (!Cmd[cmd]) {
                actions.runCommand(
                    {cmdLine: txt, currentPage: self.tabId},
                    () => {
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
        return (token && token.indexOf("--") === 0);
    }

    getFlag(flag, tokens) {
        if (!tokens || tokens.length === 0) { return null; }
        return tokens.find((token) => this.isFlag(token) && flag.toUpperCase() === token.toUpperCase());
    }

    hasFlag(flag, tokens) {
        return tokens ? tokens.find((token) => token.toUpperCase === flag.toUpperCase()) : false;
    }

    isPaging() {
        const { props } = this;
        const IS_PAGING = props.paging !== null && props.paging.pageNo <= props.paging.totalPages;
        return IS_PAGING;
    }

    isLastPage() {
        const { props } = this;
        const IS_LAST_PAGE = props.paging !== null && props.paging.pageNo === props.paging.totalPages;
        return IS_LAST_PAGE;
    }

    render() {

        const IS_PAGING = this.isPaging();
        const IS_LAST_PAGE = this.isLastPage();

        return (
            <div className={`dnn-prompt-input-wrapper ${IS_PAGING && !IS_LAST_PAGE ? "hidden-cursor" : ""}`}>
                <input className={`dnn-prompt-input ${IS_PAGING && !IS_LAST_PAGE ? "hidden-text" : ""}`} ref={(el) => {
                    this.cmdPromptInput = el;
                    if (el) {
                        el.readOnly = this.isPaging();
                        /**
                         * Note: this is a patch to be removed once a complete refactoring of the Redux
                         * implementation will be done.
                         * All changes in the DOM must happen accordingly to Redux state update and this
                         * usage of setTimeout has to be considered wrong.
                         * Patch needed to close DNN-10688
                         */
                        if (this.isPaging() && this.isLastPage() === true) {
                            el.value = "";
                            setTimeout(() => el.readOnly = false, 500);
                        }
                    }
                }} />
            </div>
        );
    }
}

Input.PropTypes = {
    nextPageCommand: PropTypes.string,
    updateHistory: PropTypes.func.isRequired,
    busy: PropTypes.func.isRequired,
    setHeight: PropTypes.func.isRequired,
    actions: PropTypes.object.isRequired,
    paging: PropTypes.bool.isRequired
};

export default Input;
