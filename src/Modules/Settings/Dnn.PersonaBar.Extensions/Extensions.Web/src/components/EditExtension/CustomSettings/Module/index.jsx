import React, { PropTypes, Component } from "react";
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
import { AddIcon } from "dnn-svg-icons";
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
        let itemsToStay = [], itemsToMove = [];
        assignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            selected ? itemsToMove.push(portal) : itemsToStay.push(portal);
        });
        props.onAssignedPortalsChange("unassignedPortals", itemsToMove, () => {
            props.onAssignedPortalsChange("assignedPortals", itemsToStay);
        });
    }
    moveItemsRight() {
        const { props } = this;
        let unassignedPortals = JSON.parse(JSON.stringify(props.extensionBeingEdited.unassignedPortals.value));
        let itemsToStay = [], itemsToMove = [];
        unassignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            selected ? itemsToMove.push(portal) : itemsToStay.push(portal);
        });

        props.onAssignedPortalsChange("assignedPortals", itemsToMove, () => {
            props.onAssignedPortalsChange("unassignedPortals", itemsToStay);
        });
    }
    moveAll(direction) {
        switch (direction) {
            case "right":

                break;
            default:

                break;
        }
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editModule}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_ModuleName.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleName.HelpText")}
                            style={inputStyle}
                            value={extensionBeingEdited.moduleName.value}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_ModuleCategory.Label")}
                            tooltipMessage={Localization.get("EditModule_ModuleCategory.HelpText")}
                            value={extensionBeingEdited.category.value}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Dependencies.Label")}
                            tooltipMessage={Localization.get("EditModule_Dependencies.HelpText")}
                            value={extensionBeingEdited.dependencies.value}
                            style={inputStyle} />
                        <Switch value={extensionBeingEdited.portable.value}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsPortable.Label")}
                            tooltipMessage={Localization.get("EditModule_IsPortable.HelpText")} />
                        <Switch value={extensionBeingEdited.upgradeable.value}
                            readOnly={true}
                            className="full-width"
                            label={Localization.get("EditModule_IsUpgradable.Label")}
                            tooltipMessage={Localization.get("EditModule_IsUpgradable.HelpText")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_FolderName.Label")}
                            tooltipMessage={Localization.get("EditModule_FolderName.HelpText")}
                            value={extensionBeingEdited.folderName.value}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_BusinessControllerClass.Label")}
                            tooltipMessage={Localization.get("EditModule_BusinessControllerClass.HelpText")}
                            value={extensionBeingEdited.businessController.value}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditModule_Permissions.Label")}
                            value={extensionBeingEdited.permissions.value}
                            tooltipMessage={Localization.get("EditModule_Permissions.HelpText")}
                            style={inputStyle} />
                        <Switch value={extensionBeingEdited.searchable.value}
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
                    <Switch value={extensionBeingEdited.premiumModule.value}
                        label={Localization.get("EditModule_IsPremiumModule.Label")}
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
                <ModuleDefinitions moduleDefinitions={extensionBeingEdited.moduleDefinitions.value} />
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
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string,
    extensionBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited
    };
}

export default connect(mapStateToProps)(Module);