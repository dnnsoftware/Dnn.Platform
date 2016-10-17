import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class ModuleDefinitionRow extends Component {
    constructor() {
        super();
        this.state = {
            isOpened: false
        };
    }
    onEdit() {
        this.setState({
            isOpened: !this.state.isOpened
        });
    }
    onDelete() {

    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <GridCell className="module-definition-row">
                <GridCell columnSize={85} className="module-definition-name">
                    Hey
                </GridCell>
                <GridCell columnSize={15} className="action-buttons">
                    <div onClick={this.onDelete.bind(this)} dangerouslySetInnerHTML={{ __html: TrashIcon }}></div>
                    <div onClick={this.onEdit.bind(this)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
                <Collapse isOpened={state.isOpened} style={{ float: "left" }} className="edit-module-definition">
                    <GridSystem>
                        <div>
                            <SingleLineInputWithError
                                label="Definition Name"
                                tooltipMessage={"Placeholder"} />
                            <SingleLineInputWithError
                                label="Default Cache Time"
                                tooltipMessage={"Placeholder"} />
                        </div>
                        <div>
                            <SingleLineInputWithError
                                label="Friendly Name"
                                tooltipMessage={"Placeholder"} />
                        </div>
                    </GridSystem>
                </Collapse>
            </GridCell>
        );
    }
}

ModuleDefinitionRow.PropTypes = {
};

export default ModuleDefinitionRow;