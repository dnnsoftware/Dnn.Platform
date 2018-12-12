import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import ControlFields from "./ControlFields";
import "./style.less";


class ControlRow extends Component {
    constructor() {
        super();
        this.state = {
            isOpened: false
        };
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <GridCell className={"module-control-row" + (props.isEditMode ? " row-opened" : "")}>
                <GridCell columnSize={15} className="module-control-title">
                    {props.moduleControl.title}
                </GridCell>
                <GridCell columnSize={70} className="module-control-source">
                    {props.moduleControl.source}
                </GridCell>
                <GridCell columnSize={15} className="action-buttons">
                    <div onClick={props.onDelete.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }}></div>
                    <div onClick={props.onEdit.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }} className={props.isEditMode ? "svg-active" : ""}></div>
                </GridCell>
                <Collapsible isOpened={props.isEditMode} style={{ float: "left" }} className="edit-module-control">
                    <ControlFields {...props} />
                </Collapsible>
            </GridCell>
        );
    }
}

ControlRow.propTypes = {
    moduleDefinition: PropTypes.object,
    moduleDefinitionBeingEdited: PropTypes.object,
    onCancel: PropTypes.func,
    onSave: PropTypes.func,
    onChange: PropTypes.func,
    onEdit: PropTypes.func
};

export default ControlRow;
