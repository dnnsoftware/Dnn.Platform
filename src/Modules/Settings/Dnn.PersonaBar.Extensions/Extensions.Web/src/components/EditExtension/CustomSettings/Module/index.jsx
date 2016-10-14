import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import AssignedSelector from "./AssignedSelector";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class Module extends Component {
    render() {
        const {props, state} = this;
        const { packageBeingEditedSettings } = props;
        return (
            <GridCell className={styles.editModule}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_ModuleName.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleName.HelpText")}
                            style={inputStyle}
                            value={packageBeingEditedSettings.moduleName}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_ModuleCategory.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleCategory.HelpText")}
                            value={packageBeingEditedSettings.category}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Dependencies.Label")}
                            tooltipMessage={Localization.get("EditModule_Dependencies.HelpText")}
                            value={packageBeingEditedSettings.dependencies}
                            style={inputStyle} />
                        <Switch value={packageBeingEditedSettings.portable}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsPortable.Label")}
                            tooltipMessage={Localization.get("EditModule_IsPortable.HelpText")} />
                        <Switch value={packageBeingEditedSettings.upgradeable}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsUpgradable.Label")}
                            tooltipMessage={Localization.get("EditModule_IsUpgradable.HelpText")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_FolderName.Label")}
                            tooltipMessage={Localization.get("EditModule_FolderName.HelpText")}
                            value={packageBeingEditedSettings.folderName}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_BusinessControllerClass.Label")}
                            tooltipMessage={Localization.get("EditModule_BusinessControllerClass.HelpText")}
                            value={packageBeingEditedSettings.businessController}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Permissions.Label")}
                            value={packageBeingEditedSettings.permissions}
                            tooltipMessage={Localization.get("EditModule_Permissions.HelpText")}
                            style={inputStyle} />
                        <Switch value={packageBeingEditedSettings.searchable}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsSearchable.Label")}
                            tooltipMessage={Localization.get("EditModule_IsSearchable.HelpText")} />
                        <DropdownWithError
                            label={Localization.get("EditModule_ModuleSharing.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleSharing.HelpText")}
                            options={[
                                {
                                    label: "Unknown",
                                    value: "Unknown"
                                },
                                {
                                    label: "Unsupported",
                                    value: "Unsupported"
                                },
                                {
                                    label: "Supported",
                                    value: "Supported"
                                }
                            ]}
                            style={inputStyle}
                            />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>
                <GridCell className="premium-module">
                    <h3 className="box-title">Premium Module Assignment</h3>
                    <Switch value={packageBeingEditedSettings.premiumModule}
                        label={Localization.get("EditModule_IsPremiumModule.Label")}
                        tooltipMessage={Localization.get("EditModule_IsPremiumModule.HelpText")} />
                    <AssignedSelector />
                </GridCell>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary">Cancel</Button>
                    <Button type="primary">{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Module.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string,
    packageBeingEditedSettings: PropTypes.object
};

export default Module;