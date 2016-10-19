import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import Button from "dnn-button";
import "./style.less";

const inputStyle = { width: "100%" };

class ModuleDefinitionRow extends Component {
    constructor() {
        super();
        this.state = {
            isOpened: false
        };
    }
    onEdit() {
    }
    onDelete() {

    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <GridCell className="module-definition-row">
                <GridCell columnSize={85} className="module-definition-name">
                    {props.moduleDefinition.name}
                </GridCell>
                <GridCell columnSize={15} className="action-buttons">
                    <div onClick={this.onDelete.bind(this)} dangerouslySetInnerHTML={{ __html: TrashIcon }}></div>
                    <div onClick={props.onEdit.bind(this)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
                <Collapse isOpened={props.isEditMode} style={{ float: "left" }} className="edit-module-definition">
                    <GridCell className="edit-module-definition-box">
                        <GridSystem>
                            <div>
                                <SingleLineInputWithError
                                    label="Definition Name"
                                    tooltipMessage={"Placeholder"}
                                    value={props.moduleDefinition.name} />
                                <SingleLineInputWithError
                                    label="Default Cache Time"
                                    tooltipMessage={"Placeholder"}
                                    value={props.moduleDefinition.cacheTime} />
                            </div>
                            <div>
                                <SingleLineInputWithError
                                    label="Friendly Name"
                                    tooltipMessage={"Placeholder"}
                                    value={props.moduleDefinition.friendlyName} />
                            </div>
                        </GridSystem>
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                            <Button type="primary">Save</Button>
                        </GridCell>
                    </GridCell>
                </Collapse>
            </GridCell>
        );
    }
}

ModuleDefinitionRow.PropTypes = {
    moduleDefinition: PropTypes.object,
    onCancel: PropTypes.func,
    onEdit: PropTypes.func
};

export default ModuleDefinitionRow;