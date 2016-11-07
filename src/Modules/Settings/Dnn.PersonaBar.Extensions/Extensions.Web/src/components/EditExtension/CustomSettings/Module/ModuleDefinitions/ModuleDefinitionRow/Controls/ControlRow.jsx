import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import Switch from "dnn-switch";
import ControlFields from "./ControlFields";
import Button from "dnn-button";
import "./style.less";

const inputStyle = { width: "100%" };

class ControlRow extends Component {
    constructor() {
        super();
        this.state = {
            isOpened: false
        };
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <GridCell className="module-control-row">
                <GridCell columnSize={15} className="module-control-title">
                    {props.moduleControl.title}
                </GridCell>
                <GridCell columnSize={70} className="module-control-source">
                    {props.moduleControl.source}
                </GridCell>
                <GridCell columnSize={15} className="action-buttons">
                    <div onClick={props.onDelete.bind(this)} dangerouslySetInnerHTML={{ __html: TrashIcon }}></div>
                    <div onClick={props.onEdit.bind(this)} dangerouslySetInnerHTML={{ __html: EditIcon }} className={props.isEditMode ? "svg-active" : ""}></div>
                </GridCell>
                <Collapse isOpened={props.isEditMode} style={{ float: "left" }} className="edit-module-control">
                    <ControlFields {...props} />
                </Collapse>
            </GridCell>
        );
    }
}

ControlRow.PropTypes = {
    moduleDefinition: PropTypes.object,
    moduleDefinitionBeingEdited: PropTypes.object,
    onCancel: PropTypes.func,
    onSave: PropTypes.func,
    onChange: PropTypes.func,
    onEdit: PropTypes.func
};

export default ControlRow;