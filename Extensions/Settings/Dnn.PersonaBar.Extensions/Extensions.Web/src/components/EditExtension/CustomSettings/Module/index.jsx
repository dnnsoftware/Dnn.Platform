import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import AssignedSelector from "./AssignedSelector";
import ModuleDefinitions from "./ModuleDefinitions";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class Module extends Component {
    onClickOnPortal(index, type) {
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        _extensionBeingEdited[type].value[index].selected = !_extensionBeingEdited[type].value[index].selected;

        props.onAssignedPortalsChange(type, _extensionBeingEdited[type].value);
    }
    moveItemsLeft() {
        const { props } = this;
        let assignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.assignedPortals.value));
        let unassignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.unassignedPortals.value));
        let itemsToStay = [], itemsToMove = [];
        let selectedCount = 0;
        assignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            if (selected) {
                selectedCount++;
                itemsToMove.push(portal);
            } else {
                itemsToStay.push(portal);
            }
        });
        if (selectedCount > 0) {
            props.onAssignedPortalsChange("unassignedPortals", unassignedPortals.concat(itemsToMove), () => {
                props.onAssignedPortalsChange("assignedPortals", itemsToStay);
            });
        }
    }
    moveItemsRight() {
        const { props } = this;
        let unassignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.unassignedPortals.value));
        let assignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.assignedPortals.value));
        let itemsToStay = [], itemsToMove = [];
        let selectedCount = 0;
        unassignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            if (selected) {
                selectedCount++;
                itemsToMove.push(portal);
            } else {
                itemsToStay.push(portal);
            }
        });
        if (selectedCount > 0) {
            props.onAssignedPortalsChange("assignedPortals", assignedPortals.concat(itemsToMove), () => {
                props.onAssignedPortalsChange("unassignedPortals", itemsToStay);
            });
        }
    }
    moveAll(direction) {
        const { props} = this;
        let assignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.assignedPortals.value));
        let unassignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.unassignedPortals.value));
        switch (direction) {
            case "right":
                props.onAssignedPortalsChange("assignedPortals", assignedPortals.concat(unassignedPortals), () => {
                    props.onAssignedPortalsChange("unassignedPortals", []);
                });
                break;
            default:
                props.onAssignedPortalsChange("unassignedPortals", unassignedPortals.concat(assignedPortals), () => {
                    props.onAssignedPortalsChange("assignedPortals", []);
                });
                break;
        }
    }
    onSelect(key, option) {
        const { props } = this;
        props.onChange(key, option.value);
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editModule + (props.className ? " " + props.className : "")}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        {!props.isAddMode && <SingleLineInputWithError
                            label={Localization.get("EditModule_ModuleName.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleName.HelpText")}
                            style={inputStyle}
                            onChange={props.onChange.bind(this, "moduleName")}
                            value={extensionBeingEdited.moduleName.value}
                            enabled={false} />
                        }
                        <DropdownWithError
                            label={Localization.get("EditModule_ModuleCategory.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleCategory.HelpText")}
                            options={props.moduleCategories.map(category => {
                                return { label: category.replace("&lt;", "<").replace("&gt;", ">"), value: category };
                            })}
                            value={extensionBeingEdited.category.value}
                            onSelect={this.onSelect.bind(this, "category")}
                            style={Object.assign({ marginBottom: 32 }, inputStyle)} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Dependencies.Label")}
                            tooltipMessage={Localization.get("EditModule_Dependencies.HelpText")}
                            value={extensionBeingEdited.dependencies.value}
                            onChange={props.onChange.bind(this, "dependencies")}
                            style={inputStyle} />
                        {props.isAddMode && <DropdownWithError
                            label={Localization.get("EditModule_ModuleSharing.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleSharing.HelpText")}
                            options={[
                                {
                                    label: Localization.get("EditModule_ModuleSharingUnknown.Label"),
                                    value: 0
                                },
                                {
                                    label: Localization.get("EditModule_ModuleSharingUnsupported.Label"),
                                    value: 1
                                },
                                {
                                    label: Localization.get("EditModule_ModuleSharingSupported.Label"),
                                    value: 2
                                }
                            ]}
                            value={extensionBeingEdited.shareable.value}
                            onSelect={this.onSelect.bind(this, "shareable")}
                            style={Object.assign({ marginBottom: 32 }, inputStyle)} />
                        }
                        <Switch value={extensionBeingEdited.portable.value}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsPortable.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            tooltipMessage={Localization.get("EditModule_IsPortable.HelpText")} />
                        <Switch value={extensionBeingEdited.upgradeable.value}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsUpgradable.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            tooltipMessage={Localization.get("EditModule_IsUpgradable.HelpText")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_FolderName.Label")}
                            tooltipMessage={Localization.get("EditModule_FolderName.HelpText")}
                            value={extensionBeingEdited.folderName.value}
                            onChange={props.onChange.bind(this, "folderName")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_BusinessControllerClass.Label")}
                            tooltipMessage={Localization.get("EditModule_BusinessControllerClass.HelpText")}
                            onChange={props.onChange.bind(this, "businessController")}
                            value={extensionBeingEdited.businessController.value}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Permissions.Label")}
                            value={extensionBeingEdited.hostPermissions.value}
                            onChange={props.onChange.bind(this, "hostPermissions")}
                            tooltipMessage={Localization.get("EditModule_Permissions.HelpText")}
                            style={inputStyle} />
                        <Switch value={extensionBeingEdited.searchable.value}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsSearchable.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            tooltipMessage={Localization.get("EditModule_IsSearchable.HelpText")} />
                        {props.isAddMode && <Switch value={extensionBeingEdited.premiumModule.value}
                            label={Localization.get("EditModule_IsPremiumModule.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            onChange={props.onChange.bind(this, "premiumModule")}
                            tooltipMessage={Localization.get("EditModule_IsPremiumModule.HelpText")} />}
                        {!props.isAddMode && <DropdownWithError
                            label={Localization.get("EditModule_ModuleSharing.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleSharing.HelpText")}
                            options={[
                                {
                                    label: "Unknown",
                                    value: 0
                                },
                                {
                                    label: "Unsupported",
                                    value: 1
                                },
                                {
                                    label: "Supported",
                                    value: 2
                                }
                            ]}
                            value={extensionBeingEdited.shareable.value}
                            onSelect={this.onSelect.bind(this, "shareable")}
                            style={inputStyle} />
                        }
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>
                {!props.isAddMode &&
                    <GridCell className="premium-module">
                        <h3 className="box-title">{Localization.get("EditModule_PremiumModuleAssignment.Header")}</h3>
                        <Switch value={extensionBeingEdited.premiumModule.value}
                            label={Localization.get("EditModule_IsPremiumModule.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            onChange={props.onChange.bind(this, "premiumModule")}
                            tooltipMessage={Localization.get("EditModule_IsPremiumModule.HelpText")} />
                        <AssignedSelector
                            assignedPortals={extensionBeingEdited.assignedPortals.value}
                            unassignedPortals={extensionBeingEdited.unassignedPortals.value}
                            onClickOnPortal={this.onClickOnPortal.bind(this)}
                            moveItemsLeft={this.moveItemsLeft.bind(this)}
                            moveItemsRight={this.moveItemsRight.bind(this)}
                            moveAll={this.moveAll.bind(this)}
                            onChange={props.onChange.bind(this)} />
                    </GridCell>
                }
                {!props.isAddMode &&
                    <ModuleDefinitions
                        extensionBeingEdited={extensionBeingEdited}
                        extensionBeingEditedIndex={props.extensionBeingEditedIndex}
                        onSave={props.onChange.bind(this, "moduleDefinitions")} />
                }
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        <Button type="primary" disabled={props.formIsDirty || props.controlFormIsDirty} onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>
                        <Button type="primary" disabled={props.formIsDirty || props.controlFormIsDirty} onClick={props.onSave.bind(this)}>{Localization.get("Save.Button")}</Button>
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Module.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    onSave: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string,
    extensionBeingEdited: PropTypes.object,
    isAddMode: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex,
        formIsDirty: state.moduleDefinition.formIsDirty,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty,
        moduleCategories: state.extension.moduleCategories
    };
}

export default connect(mapStateToProps)(Module);
