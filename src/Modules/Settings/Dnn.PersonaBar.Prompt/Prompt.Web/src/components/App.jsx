import React, { Component, PropTypes } from "react";
import Output from "./Output";
import Input from "./Input";
import { connect } from "react-redux";
import Localization from "localization";
import util from "../utils";
import "./Prompt.less";
import {
    prompt as PromptActions
} from "../actions";
import { formatString } from "../helpers";

class App extends Component {
    constructor() {
        super();
        this.isBusy = false;
        this.isPaging = false;
        this.history = [];
        this.cmdOffset = 0; // reverse offset into history
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
        this.setFocus(true);
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
            this.setFocus(true);
        } else {
            this.setFocus(true);
        }
    }

    scrollToBottom() {
        this.refs.cmdPrompt.scrollTop = this.refs.cmdPrompt.scrollHeight;
    }
    busy(b) {
        this.isBusy = b;
        this.toggleInput(!b);
    }

    showGreeting() {
        let { props } = this;
        props.dispatch(PromptActions.runLocalCommand("INFO", formatString(Localization.get('PromptGreeting'), util.version), 'cmd'));
    }
    setValue(value) {
        this.refs.cmdPromptInputControl.getWrappedInstance().setValue(value);
    }
    getValue(value) {
        return this.refs.cmdPromptInputControl.getWrappedInstance().getValue(value);
    }
    setFocus(focus) {
        this.refs.cmdPromptInputControl.getWrappedInstance().setFocus(focus);
    }
    toggleInput(show) {
        this.refs.cmdPromptInputControl.getWrappedInstance().toggleInput(show);
        this.setFocus(show);
    }
    runCmd() {
        this.refs.cmdPromptInputControl.getWrappedInstance().runCmd();
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
        if (this.refs.cmdPrompt !== undefined && height !== undefined && height !== "") {
            height = height.replace("%", "");
            if (parseInt(height) > 0 && parseInt(height) <= 100)
                this.refs.cmdPrompt.style.height = height + "%";
        }
    }

    onKeyDown(e) {
        let { props } = this;
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
                props.dispatch(PromptActions.endPaging());
                this.toggleInput(true);
                this.setFocus(true);
                return;
            }
        }

        if (this.isBusy) return;
        if (document.activeElement.className === "dnn-prompt-input" && document.activeElement.offsetParent !== null
            && document.activeElement.offsetParent.className === "dnn-prompt-input-wrapper" && document.activeElement.tagName === "INPUT"
            && document.activeElement.type === "text") {
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
        if (this.isPaging && !e.ctrlKey) {
            return this.runCmd();
        }
    }

    render() {
        return (
            <div className="dnn-prompt" style={{ display: "block" }} ref="cmdPrompt">
                <Output className="Output" scrollToBottom={this.scrollToBottom.bind(this)}
                    busy={this.busy.bind(this)} toggleInput={this.toggleInput.bind(this)} IsPaging={this.paging.bind(this)}></Output>
                <br />
                <Input ref="cmdPromptInputControl" updateHistory={this.updateHistory.bind(this)} busy={this.busy.bind(this)} paging={this.paging.bind(this)} setHeight={this.setHeight.bind(this)} />

            </div >
        );
    }
}
App.PropTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
}

export default connect(mapStateToProps)(App);