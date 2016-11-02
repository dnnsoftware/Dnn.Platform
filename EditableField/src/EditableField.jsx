import React, {PropTypes, Component} from "react";
import ReactDOM from "react-dom";
import TextArea from "dnn-multi-line-input";
import Input from "dnn-single-line-input";
import {EditIcon} from "dnn-svg-icons";
import "./style.less";


class EditableField extends Component {
    constructor() {
        super();
        this.uniqueId = "editableField-" + (Date.now() * Math.random());
    }

    componentWillMount() {
        const {props} = this;
        this.setState({
            editMode: false,
            value: props.value
        });
    }
    toggleEditMode() {
        if (!this.state.editMode) {
            ReactDOM.findDOMNode(this.refs.editableInput).focus();
        }
        this.setState({
            editMode: !this.state.editMode
        });
    }

    getEditableLabelClass() {
        const {props, state} = this;

        let editableLabelClass = state.editMode ? " in-edit" : "";
        let inputTypeClass = props.inputType === "textArea" ? " textArea" : " singleLine";

        return "dnn-editable-field" + editableLabelClass + inputTypeClass;
    }

    onFocus() {
        window.dnn.stopEscapeFromClosingPB = true;
    }
    getInputFromType() {
        const {props, state} = this;
        if (props.inputType && props.inputType === "textArea") {
            return <TextArea
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this) }
                onChange={this.onKeyUp.bind(this) }
                onBlur={this.onBlur.bind(this) }
                onFocus={this.onFocus.bind(this) }
                ref="editableInput"
                enabled={state.editMode}
                />;
        } else {
            return <Input
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this) }
                onChange={this.onKeyUp.bind(this) }
                onBlur={this.onBlur.bind(this) }
                onFocus={this.onFocus.bind(this) }
                ref="editableInput"
                enabled={state.editMode}
                />;
        }
    }

    onKeyDown(event) {
        const {props, state} = this;
        switch (event.keyCode) {
            case 13:
                event.preventDefault();
                props.onEnter(state.value);
                ReactDOM.findDOMNode(this.refs.editableInput).blur();
                this.setState({
                    editMode: false
                });
                //enter
                break;
            case 27:
                event.preventDefault();
                //escape
                this.setState({
                    value: props.value,
                    editMode: false
                });
                setTimeout(() => {
                    ReactDOM.findDOMNode(this.refs.editableInput).blur();
                }, 250);
                break;
            default:
                break;
        }
    }
    onBlur() {
        window.dnn.stopEscapeFromClosingPB = false;
    }
    onKeyUp(event) {
        const value = event.target.value;
        this.setState({
            value
        });
    }

    /* eslint-disable react/no-danger */
    getEditButton() {
        return <div className="edit-button" onClick={this.toggleEditMode.bind(this) } dangerouslySetInnerHTML={{ __html: EditIcon }}></div>;
    }

    getEditableValue() {
        const {state} = this;
        return <span className="editable-value">{state.value}</span>;
    }

    render() {
        const {props, refs} = this;
        const editableLabelClass = this.getEditableLabelClass();
        const input = this.getInputFromType();
        const editableValue = this.getEditableValue();
        const editButton = this.getEditButton();
        return (
            <div className={editableLabelClass}>
                <label className="editable-label" htmlFor={refs.input}>{props.label}</label>
                {props.editable && editButton}
                {editableValue}
                <div className="editable-input">
                    {input}
                    <span className="help-text">{(props.helpText || "Press ENTER when done, or ESC to cancel") }</span>
                </div>
            </div>
        );
    }
}

EditableField.PropTypes = {
    label: PropTypes.string,
    value: PropTypes.string,
    inputType: PropTypes.string,
    onEnter: PropTypes.func,
    editable: PropTypes.bool,
    helpText: PropTypes.string
};

EditableField.defaultProps = {
    editable: true
};

export default EditableField;