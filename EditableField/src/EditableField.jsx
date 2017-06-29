import React, { PropTypes, Component } from "react";
import ReactDOM from "react-dom";
import TextArea from "dnn-multi-line-input-with-error";
import Input from "dnn-single-line-input-with-error";
import { EditIcon } from "dnn-svg-icons";
import "./style.less";


class EditableField extends Component {
    constructor() {
        super();
        this.uniqueId = "editableField-" + (Date.now() * Math.random());
    }

    componentWillMount() {
        const { props } = this;
        this.setState({
            editMode: false,
            value: props.value,
            error: false
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
        const { props, state } = this;

        let editableLabelClass = state.editMode ? " in-edit" : "";
        let inputTypeClass = props.inputType === "textArea" ? " textArea" : " singleLine";

        return "dnn-editable-field" + editableLabelClass + inputTypeClass;
    }

    onFocus() {
        window.dnn.stopEscapeFromClosingPB = true;
        if (typeof this.props.onFocus === "function") {
            this.props.onFocus();
        }
    }
    getInputFromType() {
        const { props, state } = this;
        if (props.inputType && props.inputType === "textArea") {
            return <TextArea
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this)}
                onChange={this.onKeyUp.bind(this)}
                onBlur={this.onBlur.bind(this)}
                onFocus={this.onFocus.bind(this)}
                ref="editableInput"
                enabled={state.editMode}
                error={state.error}
                errorMessage={props.errorMessage}
            />;
        } else {
            return <Input
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this)}
                onChange={this.onKeyUp.bind(this)}
                onBlur={this.onBlur.bind(this)}
                onFocus={this.onFocus.bind(this)}
                ref="editableInput"
                enabled={state.editMode}
                error={state.error}
                errorMessage={props.errorMessage}
            />;
        }
    }

    onKeyDown(event) {
        const { props, state } = this;
        if (typeof this.props.onKeyDown === "function") {
            this.props.onKeyDown(event);
        }
        switch (event.keyCode) {
            case 13:
                event.preventDefault();
                if (props.enableCallback) {
                    props.onEnter(state.value, () => {
                        ReactDOM.findDOMNode(this.refs.editableInput).blur();
                        this.setState({
                            editMode: false,
                            error: false
                        });
                    }, () => {
                        this.setState({
                            error: true
                        });
                    });
                }
                else {
                    props.onEnter(state.value);
                    ReactDOM.findDOMNode(this.refs.editableInput).blur();
                    this.setState({
                        editMode: false,
                        error: false
                    });
                }
                //enter
                break;
            case 27:
                event.preventDefault();
                //escape
                this.setState({
                    value: props.value,
                    editMode: false,
                    error: false
                });
                if (typeof this.props.onEscape === "function") {
                    this.props.onEscape(event);
                }
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
        if (typeof this.props.onBlur === "function") {
            this.props.onBlur();
        }
    }
    onKeyUp(event) {
        const value = event.target.value;
        this.setState({
            value
        });
        if (typeof this.props.onKeyUp === "function") {
            this.props.onKeyUp(event);
        }
    }

    /* eslint-disable react/no-danger */
    getEditButton() {
        return <div className="edit-button" onClick={this.toggleEditMode.bind(this)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>;
    }

    getUrl(text) {
        if (text.indexOf("http://") !== 0 && text.indexOf("https://") !== 0) {
            return "http://" + text;
        }
        return text;
    }

    getEditableValue() {
        const { props, state } = this;
        if (!props.isUrl) {
            return <span className="editable-value">{state.value}</span>;
        }
        return <span className="editable-value"><a href={this.getUrl(state.value)} target="_blank">{state.value}</a></span>;
    }

    render() {
        const { props, refs } = this;
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
                    <span className="help-text">{(props.helpText || "Press ENTER when done, or ESC to cancel")}</span>
                </div>
            </div>
        );
    }
}

EditableField.propTypes = {
    label: PropTypes.string,
    value: PropTypes.string,
    inputType: PropTypes.string,
    onEnter: PropTypes.func,
    editable: PropTypes.bool,
    helpText: PropTypes.string,
    isUrl: PropTypes.bool.isRequired,
    onKeyUp: PropTypes.func,
    onKeyDown: PropTypes.func,
    onBlur: PropTypes.func,
    onFocus: PropTypes.func,
    onEscape: PropTypes.func,
    errorMessage: PropTypes.string,
    enableCallback: PropTypes.bool
};

EditableField.defaultProps = {
    editable: true,
    isUrl: false,
    enableCallback: false
};

export default EditableField;