import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import GridCell from "dnn-grid-cell";
import Output from "./Output";
import Input from "./Input";
import { connect } from "react-redux";
import Localization from "localization";
import util from "../utils";
import "./Prompt.less";
import {
    prompt as PromptActions
} from "../actions";

class App extends Component {
    constructor() {
        super();
        this.isBusy = false;
        this.history = [];
        this.keyDownHandler = this.onKeyDown.bind(this);
        this.clickHandler = this.onClickHandler.bind(this);
        this.mouseDownHandler = this.onMouseDownHandler.bind(this);
        this.mouseUpHandler = this.onMouseUpHandler.bind(this);
        this.isDragging = false;
    }
    componentDidMount() {
        document.addEventListener('keydown', this.keyDownHandler);
        document.addEventListener('mousedown', this.mouseDownHandler);
        document.addEventListener('mouseup', this.mouseUpHandler);
        document.addEventListener('click', this.clickHandler); this._isMounted = true;
        this._isMounted = true;
        this.focus();
    }
    componentWillMount() {
        this.showGreeting();
    }
    componentWillUnmount() {
        document.removeEventListener('keydown', this.keyDownHandler);
        document.removeEventListener('mousedown', this.mouseDownHandler);
        document.removeEventListener('mouseup', this.mouseUpHandler);
        document.removeEventListener('click', this.clickHandler);
        this._isMounted = false;
    }
    onMouseDownHandler(e) {
        this.mouseX = e.clientX;
        this.mouseY = e.clientY;
    }
    onMouseUpHandler(e) {
        if (Math.abs(this.mouseX - e.clientX) > 10 || Math.abs(this.mouseY - e.clientY) > 5) {
            this.isDragging = true;
        } else {
            this.isDragging = false;
        }
    }
    onClickHandler(e) {
        if (this.isDragging) return;
        if (e.target.classList.contains("dnn-prompt-cmd-insert")) {
            this.setValue(e.target.dataset.cmd.replace(/'/g, '"'));
            this.toggleInput(true);
            this.focus();
        } else {
            this.focus();
        }
    }
    focus() {
        this.refs.cmdPromptInput.focus();
    }
    getTabId() {
        const dnnVariable = JSON.parse(window.parent.document.getElementsByName("__dnnVariable")[0].value);
        return dnnVariable.sf_tabId;
    }
    scrollToBottom() {
        this.scrollTop = this.scrollHeight;
    }
    busy(b) {
        this.isBusy = b;
        //this.busyEl.style.display = b ? "block" : "none";
        this.toggleInput(!b);
    }
    runCmd() {
        let self = this;
        const txt = self.refs.cmdPromptInput.value.trim();
        let { props } = self;
        if (!self.tabId) {
            self.tabId = self.getTabId();
        }

        self.cmdOffset = 0; // reset history index
        self.setValue(""); // clearn input for future commands.

        if (txt === "") {
            return;
        } // don't process if cmd is emtpy
        props.dispatch(PromptActions.runLocalCommand("INFO", txt, 'cmd'));
        self.history.push(txt); // Add cmd to history
        if (sessionStorage) {
            sessionStorage.setItem('dnn-prompt-console-history', JSON.stringify(self.history));
        }

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
            props.dispatch(PromptActions.runLocalCommand(cmd, "Session command history cleared"));
            return;
        }
        if (cmd === "CONFIG") {
            self.configConsole(tokens);
            return;
        }

        if (cmd === "SET-MODE") {
            self.changeUserMode(tokens);
            return;
        }
        // Server Command
        self.busy(true);
        if (cmd === "HELP") {
            props.dispatch(PromptActions.runHelpCommand({ cmdLine: txt, currentPage: self.tabId }, () => {
                self.busy(false);
                self.focus();
            }, (error) => {
                props.dispatch(PromptActions.runLocalCommand("ERROR", error.responseJSON.Message));
                self.busy(false);
                self.focus();
            }));
        } else {
            props.dispatch(PromptActions.runCommand({ cmdLine: txt, currentPage: self.tabId }, () => { }, (error) => {
                props.dispatch(PromptActions.runLocalCommand("ERROR", error.responseJSON.Message));
                self.busy(false);
                self.focus();
            }));
        }
        self.refs.cmdPromptInput.blur(); // remove focus from input elment

    }
    showGreeting() {
        let { props } = this;
        props.dispatch(PromptActions.runLocalCommand("INFO", 'Prompt [' + util.version + '] Type \'help\' to get a list of commands', 'cmd'));
    }
    setValue(value) {
        this.refs.cmdPromptInput.value = value;
    }
    toggleInput(show) {
        this.refs.cmdPromptInputDiv.style.display = show ? "block" : "none";
    }
    onKeyDown(e) {
        // CTRL + `
        if (e.ctrlKey) {
            if (e.keyCode === 192) {
                if (this.wrapper[0].offsetLeft <= 0) {
                    this.util.loadPanel("Dnn.Prompt", {
                        moduleName: "Dnn.Prompt",
                        folderName: "",
                        identifier: "Dnn.Prompt",
                        path: "Prompt"
                    });
                } else {
                    this.util.closePersonaBar();
                }
                return;
            }
            if (e.keyCode === 88) {
                this.toggleInput(true);
                this.focus();
                //End paging. Return to prompt.
                //ShowPrompt();
                return;
            }
        }

        if (this.isBusy) return;
        if (this.refs.cmdPromptInput === document.activeElement) {
            switch (e.keyCode) {
                case 13: // enter key
                    return this.runCmd();
                case 38: // Up arrow
                    if ((this.history.length + this.cmdOffset > 0)) {
                        this.cmdOffset--;
                        this.setValue(this.history[this.history.length + this.cmdOffset]);
                        e.preventDefault();
                    }
                    break;
                case 40: // Down arrow
                    if ((this.cmdOffset < -1)) {
                        this.cmdOffset++;
                        this.setValue(this.history[this.history.length + this.cmdOffset]);
                        e.preventDefault();
                    }
                    break;
            }
        }
    }

    render() {
        return (
            <div className="dnn-prompt workspace" style={{ display: "block" }}>
                <Output className="Output" scrollToBottom={this.scrollToBottom.bind(this)}
                    busy={this.busy.bind(this)} focus={this.focus.bind(this)} toggleInput={this.toggleInput.bind(this)}></Output>
                <br />
                <div className="dnn-prompt-input-wrapper" ref="cmdPromptInputDiv">
                    <input className="dnn-prompt-input" ref="cmdPromptInput" />
                </div>
            </div >
        );
    }
}
App.PropTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {};
}

export default connect(mapStateToProps)(App);