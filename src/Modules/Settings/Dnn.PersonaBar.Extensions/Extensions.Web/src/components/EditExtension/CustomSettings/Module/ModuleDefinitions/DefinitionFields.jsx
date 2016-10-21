import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Button from "dnn-button";
import Localization from "localization";
import { AddIcon } from "dnn-svg-icons";
import ModuleDefinitionRow from "./ModuleDefinitionRow";
import Collapse from "react-collapse";
import utilities from "utils";
import { ModuleDefinitionActions } from "actions";
import styles from "./style.less";


function removeRecordFromArray(arr, index) {
    return [...arr.slice(0, index), ...arr.slice(index + 1)];
}
class DefinitionFields extends Component {
    constructor() {
        super();
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;

        return (
                <GridSystem>
                    <div>
                        <SingleLineInputWithError
                            label="Definition Name*"
                            tooltipMessage={"Placeholder"}
                            onChange={props.onChange.bind(this, "name")}
                            enabled={!props.isEditMode}
                            error={props.error.name && props.triedToSave}
                            value={props.moduleDefinitionBeingEdited.name} />
                        <SingleLineInputWithError
                            label="Default Cache Time"
                            tooltipMessage={"Placeholder"}
                            onChange={props.onChange.bind(this, "cacheTime")}
                            value={props.moduleDefinitionBeingEdited.cacheTime} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label="Friendly Name*"
                            tooltipMessage={"Placeholder"}
                            error={props.error.friendlyName && props.triedToSave}
                            onChange={props.onChange.bind(this, "friendlyName")}
                            value={props.moduleDefinitionBeingEdited.friendlyName} />
                    </div>
                </GridSystem>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

DefinitionFields.PropTypes = {
    onChange: PropTypes.func,
    error: PropTypes.object,
    triedToSave: PropTypes.bool,
    isEditMode: PropTypes.bool,
    moduleDefinitionBeingEdited: PropTypes.object
};

export default DefinitionFields;