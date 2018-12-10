import React, { Component } from "react";
import PropTypes from "prop-types";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Localization from "localization";
import "./style.less";
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
                        label={Localization.get("ModuleDefinitions_DefinitionName.Label") + "*"}
                        tooltipMessage={Localization.get("ModuleDefinitions_DefinitionName.HelpText")}
                        onChange={props.onChange.bind(this, "name")}
                        enabled={!props.isEditMode}
                        error={props.error.name && props.triedToSave}
                        value={props.moduleDefinitionBeingEdited.name} />
                    <SingleLineInputWithError
                        label={Localization.get("ModuleDefinitions_DefaultCacheTime.Label")}
                        tooltipMessage={Localization.get("ModuleDefinitions_DefaultCacheTime.HelpText")}
                        onChange={props.onChange.bind(this, "cacheTime")}
                        value={props.moduleDefinitionBeingEdited.cacheTime} />
                </div>
                <div>
                    <SingleLineInputWithError
                        label={Localization.get("ModuleDefinitions_FriendlyName.Label") + "*"}
                        tooltipMessage={Localization.get("ModuleDefinitions_FriendlyName.HelpText")}
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
