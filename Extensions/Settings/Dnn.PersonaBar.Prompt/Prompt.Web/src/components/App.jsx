import React, { Component } from "react";
import PropTypes from "prop-types";
import Output from "components/Output";
import Input from "components/Input";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Localization from "localization/Localization";
import { util, formatString } from "utils/helpers";
import "components/Prompt.less";
import * as PromptActionsCreators from "actions/prompt";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";

export class App extends Component {

    constructor(props) {
        super(props);
        this.isPaging = false;
        this.history = [];
        this.cmdOffset = 0; // reverse offset into history
        const { dispatch } = props;
        this.actions = bindActionCreators(PromptActionsCreators, dispatch);
    }
    componentDidUpdate() {
        this.setFocus(true);
        this.scrollToBottom();
    }
    componentDidMount() {
        this.showGreeting();
    }

    onClickHandler(e) {
        if (e.target.classList.contains("dnn-prompt-cmd-insert")) {
            this.setValue(e.target.dataset.cmd.replace(/'/g, "\""));
            this.setFocus(true);
        } else {
            this.setFocus(true);
        }
    }

    scrollToBottom() {
        if (this.cmdPrompt) {
            this.cmdPrompt.scrollTop = this.cmdPrompt.scrollHeight;
        }
    }

    showGreeting() {
        this.actions.runLocalCommand("INFO", formatString(Localization.get("PromptGreeting"), util.version), "cmd");
    }
    setValue(value) {
        this.cmdPromptInputControl.setValue(value);
    }
    setFocus(focus) {
        this.cmdPromptInputControl.setFocus(focus);
    }

    runCmd() {
        this.cmdPromptInputControl.runCmd();
    }
    updateHistory(value, isClear) {
        this.cmdOffset = 0; // reset history index
        if (isClear) {
            this.history = [];
        } else {
            //Remove command from history if already exists. 
            if (this.history.some(item => item === value)) {
                this.history = this.history.filter(item => {
                    return item !== value;
                });
            }
            //Push the command as the last run.
            this.history.push(value);
        }
    }
    paging(isPaging) {
        this.isPaging = isPaging;
    }
    setHeight(height) {
        if (this.cmdPrompt !== null && this.cmdPrompt !== undefined && height !== undefined && height !== "") {
            height = height.replace("%", "");
            if (parseInt(height) > 0 && parseInt(height) <= 100)
                this.cmdPrompt.style.height = height + "%";
        }
    }

    endPaging() {
        this.actions.endPaging();
        this.setValue("");
        this.setFocus(true);
    }

    keyDownHandler(e) {
        const { props } = this;

        const lastPage = props.paging !== null && props.paging.pageNo === props.paging.totalPages;

        //CTRL + Key
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
                this.endPaging();
                return;
            }
        }

        if (props.isBusy) return;
        if (
            document.activeElement.className.indexOf("dnn-prompt-input") > -1
            && document.activeElement.offsetParent !== null
            && document.activeElement.offsetParent.className.indexOf("dnn-prompt-input-wrapper") > -1
            && document.activeElement.tagName === "INPUT"
            && document.activeElement.type === "text") {
            switch (e.keyCode) {
                case 13: // enter key
                    if (this.isPaging) {
                        this.setValue("");
                        this.runCmd();
                        this.setFocus(false);
                        return;
                    }
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

        if (this.isPaging && !e.ctrlKey) {
            this.setValue("");
            this.setFocus(false);
            this.runCmd();
            if (lastPage) {
                this.endPaging();
            }
            return;
        }
    }

    render() {
        const { props } = this;
        return (
            <PersonaBarPage isOpen={true} fullWidth={true}>
                <div className="wrapper">
                    <div
                        className="dnn-prompt"
                        style={{ display: "block" }}
                        onKeyDown={this.keyDownHandler.bind(this)}
                        onClick={this.onClickHandler.bind(this)}
                        ref={(el) => this.cmdPrompt = el}>
                        <Output
                            {...props}
                            className="Output"
                            scrollToBottom={this.scrollToBottom.bind(this)}
                            IsPaging={this.paging.bind(this)}></Output>
                        <br />
                        {!props.isBusy && <Input
                            ref={(el) => this.cmdPromptInputControl = el}
                            {...props}
                            actions={this.actions}
                            updateHistory={this.updateHistory.bind(this)}
                            paging={props.paging}
                            setHeight={this.setHeight.bind(this)} />}
                    </div>
                </div>
            </PersonaBarPage>
        );
    }
}
App.propTypes = {
    output: PropTypes.string,
    data: PropTypes.array,
    isHtml: PropTypes.bool,
    reload: PropTypes.bool,
    isError: PropTypes.bool,
    clearOutput: PropTypes.bool,
    fieldOrder: PropTypes.array,
    commandList: PropTypes.array,
    style: PropTypes.string,
    isBusy: PropTypes.bool,
    isHelp: PropTypes.bool,
    name: PropTypes.string,
    nextPageCommand: PropTypes.string,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string,
    error: PropTypes.string,
};

function mapStateToProps(state) {
    return {
        output: state.output,
        data: state.data,
        paging: state.pagingInfo,
        isBusy: state.isBusy,
        isHtml: state.isHtml,
        reload: state.reload,
        style: state.style,
        fieldOrder: state.fieldOrder,
        commandList: state.commandList,
        isError: state.isError,
        clearOutput: state.clearOutput,
        isHelp: state.isHelp,
        name: state.name,
        description: state.description,
        options: state.options,
        resultHtml: state.resultHtml,
        error: state.error,
        nextPageCommand: state.nextPageCommand
    };
}

export default connect(mapStateToProps)(App);