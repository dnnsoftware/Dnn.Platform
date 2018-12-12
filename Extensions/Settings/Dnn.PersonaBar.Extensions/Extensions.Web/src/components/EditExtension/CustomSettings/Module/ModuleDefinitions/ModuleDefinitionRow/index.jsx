import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, Collapsible, SvgIcons, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import Controls from "./Controls";
import DefinitionFields from "../DefinitionFields";
import "./style.less";


class ModuleDefinitionRow extends Component {
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
            <GridCell className={"module-definition-row " + (props.isEditMode ? " row-opened": "")}>
                <GridCell columnSize={85} className="module-definition-name">
                    {props.moduleDefinition.name}
                </GridCell>
                <GridCell columnSize={15} className="action-buttons">
                    <div onClick={props.onDelete.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }}></div>
                    <div onClick={props.onEdit.bind(this)} className={props.isEditMode ? "svg-active" : ""} dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }}></div>
                </GridCell>
                <Collapsible isOpened={props.isEditMode} style={{ float: "left" }} className="edit-module-definition">
                    <GridCell className="edit-module-definition-box">
                        <DefinitionFields
                            error={props.error}
                            triedToSave={props.triedToSave}
                            onChange={props.onChange.bind(this)}
                            isEditMode={true}
                            moduleDefinitionBeingEdited={props.moduleDefinitionBeingEdited} />
                        <GridCell className="module-controls">
                            <Controls
                                moduleControls={props.moduleDefinitionBeingEdited.controls}
                                extensionBeingEdited={props.extensionBeingEdited}
                                extensionBeingEditedIndex={props.extensionBeingEditedIndex}
                                onChange={props.onChange.bind(this, "controls")}
                                onControlSave={props.onSave.bind(this)}
                                moduleDefinitionId={props.moduleDefinitionBeingEdited.id} />
                        </GridCell>
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("ModuleDefinitions_Cancel.Button")}</Button>
                            <Button type="primary" disabled={props.controlFormIsDirty} onClick={props.onSave.bind(this)}>{Localization.get("ModuleDefinitions_Save.Button")}</Button>
                        </GridCell>
                    </GridCell>
                </Collapsible>
            </GridCell>
        );
    }
}

ModuleDefinitionRow.propTypes = {
    moduleDefinition: PropTypes.object,
    moduleDefinitionBeingEdited: PropTypes.object,
    onCancel: PropTypes.func,
    onSave: PropTypes.func,
    onChange: PropTypes.func,
    onEdit: PropTypes.func,
    error: PropTypes.object
};

export default ModuleDefinitionRow;
