import React, {PropTypes, Component} from "react";
import TextArea from "dnn-multi-line-input";
import Input from "dnn-single-line-input";
import "./style.less";

/* eslint-disable quotes */
const EditSvg = require(`!raw!../../../img/common/edit.svg`);


class EditableWithLabel extends Component {
    constructor() {
        super();
    }

    componentWillMount() {
        const {props} = this;
        this.setState({
            editMode: false,
            value: props.value
        });
    }

    toggleEditMode() {
        this.setState({
            editMode: !this.state.editMode
        });
    }

    getEditableLabelClass() {
        const {props, state} = this;

        let editableLabelClass = state.editMode ? " in-edit" : "";
        let inputTypeClass = props.inputType === "textArea" ? " textArea" : " singleLine";

        return "editable-with-label" + editableLabelClass + inputTypeClass;
    }

    getInputFromType() {
        const {props, state} = this;
        if (props.inputType && props.inputType === "textArea") {
            return <TextArea
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this) }
                onChange={this.onKeyUp.bind(this) }
                />;
        } else {
            return <Input
                value={state.value}
                onChange={this.onKeyDown.bind(this) }
                />;
        }
    }

    onKeyDown(event) {
        const {props, state} = this;
        switch (event.keyCode) {
            case 13:
                event.preventDefault();
                props.onEnter(state.value);
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
                break;
            default:
                break;
        }
    }
    onKeyUp(event) {
        const value = event.target.value;
        this.setState({
            value
        });
    }

    /* eslint-disable react/no-danger */
    getEditButton() {
        return <div className="edit-button" onClick={this.toggleEditMode.bind(this) } dangerouslySetInnerHTML={{ __html: EditSvg }}></div>;
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
                    <span className="help-text">{__("Press ENTER when done, or ESC to cancel") }</span> 
                </div>
            </div>
        );
    }
}

EditableWithLabel.PropTypes = {
    label: PropTypes.string,
    value: PropTypes.string,
    inputType: PropTypes.string,
    onEnter: PropTypes.func,
    editable: PropTypes.bool
};

EditableWithLabel.defaultProps = {
    editable: true
};

export default EditableWithLabel;