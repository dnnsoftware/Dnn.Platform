import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import GridCell from "dnn-grid-cell";
import { connect } from "react-redux";
import Localization from "localization";
import util from "../../utils";
import "../Prompt.less";

class Input extends Component {
    constructor() {
        super();
        this.state = {
            inputValue: ""
        };
        this.keyDownHandler = this.onKeyDown.bind(this);
    }
    componentDidMount() {
        document.addEventListener('keydown', this.keyDownHandler);
    }

    componentWillUnmount() {
        document.removeEventListener('keydown', this.keyDownHandler);
    }
    getTabId() {
        const dnnVariable = JSON.parse(window.parent.document.getElementsByName("__dnnVariable"));
        return dnnVariable.sf_tabId;
    }
    writeLine(txt, cssSuffix) {
        let { props } = this;
        let span = document.createElement('span');
        cssSuffix = cssSuffix || 'ok';
        span.className = 'dnn-prompt-' + cssSuffix;
        span.innerText = txt;
        props.pushToOutput(span);
        this.newLine();
    }
    newLine() {
        let { props } = this;
        props.pushToOutput(document.createElement('br'));
        props.scrollToBottom();
    }
    runCmd() {
        const txt = this.state.inputValue.trim();
        let { props } = this;
        if (!this.tabId) {
            this.tabId = this.getTabId();
        }

        this.cmdOffset = 0; // reset history index
        this.setValue(""); // clearn input for future commands.
        this.writeLine(txt, "cmd"); // Write cmd to output
        if (txt === "") {
            return;
        } // don't process if cmd is emtpy
        this.history.push(txt); // Add cmd to history
        if (sessionStorage) {
            sessionStorage.setItem('dnn-prompt-console-history', JSON.stringify(this.history));
        }

        // Client Command
        const tokens = txt.split(" "),
            cmd = tokens[0].toUpperCase();

        if (cmd === "CLS" || cmd === "CLEAR-SCREEN") {
            props.setOutput("");
            return;
        }
        if (cmd === "EXIT") {
            util.closePersonaBar();
            return;
        }
        if (cmd === "HELP") {
            this.renderHelp(tokens);
            return;
        }
        if (cmd === "CONFIG") {
            this.configConsole(tokens);
            return;
        }
        if (cmd === "CLH" || cmd === "CLEAR-HISTORY") {
            this.history = [];
            sessionStorage.removeItem('dnn-prompt-console-history');
            this.writeLine("Session command history cleared");
            return;
        }
        if (cmd === "SET-MODE") {
            this.changeUserMode(tokens);
            return;
        }
        // using if/else to allow reload if hash in URL and also prevent 'syntax invalid' message;
        if (cmd === "RELOAD") {
            window.top.location.reload(true);
        } else {
            // Server Command
            this.busy(true);
            // special handling for 'goto' command
            let bRedirect = false;
            if (cmd === "GOTO") {
                bRedirect = true;
            }

            const afVal = util.sf.antiForgeryToken;

            let path = 'API/PersonaBar/Command/Cmd';
            if (util.sf) {
                path = util.sf.getSiteRoot() + path;
            } else {
                path = '/' + path;
            }

            fetch(path, {
                method: 'post',
                headers: new Headers({
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': afVal
                }),
                credentials: 'include',
                body: JSON.stringify({ cmdLine: txt, currentPage: this.tabId })
            })
                .then(function (response) {
                    return response.json();
                })
                .then(function (result) {
                    if (result.Message) {
                        // dnn web api error
                        result.output = result.Message;
                        result.isError = true;
                    }
                    const output = result.output;
                    const style = result.isError ? "error" : "ok";
                    const data = result.data;
                    let fieldOrder = result.fieldOrder;
                    if (typeof fieldOrder === 'undefined' || !fieldOrder || fieldOrder.length === 0) {
                        fieldOrder = null;
                    }

                    if (bRedirect) {
                        window.top.location.href = output;
                    } else {
                        if (data) {
                            let html = this.renderData(data, fieldOrder);
                            this.writeHtml(html);
                            if (output) { this.writeLine(output); }
                        } else if (result.isHtml) {
                            this.writeHtml(output);
                        } else {
                            this.writeLine(output, style);
                        }
                    }

                    if (result.mustReload) {
                        this.writeHtml('<div class="dnn-prompt-ok"><strong>Reloading in 3 seconds</strong></div>');
                        setTimeout(() => location.reload(true), 3000);
                    }
                })
                .catch(function (err) {
                    console.log('err', err);
                    this.writeLine("Error sending request to server", "error");
                })
                .then(function () {
                    // finally
                    this.busy(false);
                    this.focus();
                });

            this.inputEl.blur(); // remove focus from input elment
        }

    }
    onClick(value) {
        this.setValue(value);
    }
    setValue(value) {
        let { state } = this;
        state.inputValue = value;
        this.setState(this, () => {
            this.focus();
        });
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
                //End paging. Return to prompt.
                //ShowPrompt();
                return;
            }
        }

        if (this.isBusy) return;
        if (this === document.activeElement) {
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
            <input className="dnn-prompt-input" />
        );
    }
}
Input.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    onMouseDown: PropTypes.func.isRequired,
    onMouseUp: PropTypes.func.isRequired,
    onClick: PropTypes.func.isRequired,
    onKeyDown: PropTypes.func.isRequired,
    pushToOutput: PropTypes.func.isRequired,
    setOutput: PropTypes.func.isRequired,
    scrollToBottom: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {};
}

export default connect(mapStateToProps)(Input);