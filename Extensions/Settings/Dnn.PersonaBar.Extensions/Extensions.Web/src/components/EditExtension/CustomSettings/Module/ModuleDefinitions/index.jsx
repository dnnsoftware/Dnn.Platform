import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions } from "actions";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import Localization from "localization";
import { AddIcon } from "dnn-svg-icons";
import ModuleDefinitionRow from "./ModuleDefinitionRow";
import Collapse from "dnn-collapsible";
import utilities from "utils";
import { ModuleDefinitionActions } from "actions";
import DefinitionFields from "./DefinitionFields";
import utils from "utils";
import "./style.less";

class ModuleDefinitions extends Component {
    constructor() {
        super();
        this.state = {
            moduleDefinitionBeingEdited: {},
            moduleDefinitionBeingEditedIndex: null,
            error: {
                name: false,
                friendlyName: false
            },
            triedToSave: false,
            editMode: false
        };
    }
    getNewModuleDefinition() {
        const { props } = this;
        return {
            id: -1,
            desktopModuleId: props.desktopModuleId,
            name: "",
            friendlyName: "",
            cacheTime: 0
        };
    }
    UNSAFE_componentWillMount() {
        this.setState({
            moduleDefinitionBeingEdited: this.getNewModuleDefinition()
        });
    }
    confirmAction(callback) {
        const { props } = this;
        if (props.formIsDirty) {
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setFormDirt(false));
                if (props.controlFormIsDirty) {
                    props.dispatch(ModuleDefinitionActions.setControlFormDirt(false));
                }
            });
        } else {
            callback();
        }
    }
    resetError() {
        return {
            name: false,
            friendlyName: false
        };
    }
    exitEditMode() {
        this.confirmAction(() => {
            this.setState({
                moduleDefinitionBeingEdited: this.getNewModuleDefinition(),
                error: this.resetError(),
                moduleDefinitionBeingEditedIndex: null,
                editMode: false,
                triedToSave: false
            });
        });
    }
    _onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
        this.setState({
            editMode: true,
            error: {
                name: moduleDefinitionBeingEdited.name === "",
                friendlyName: moduleDefinitionBeingEdited.friendlyName === ""
            },
            moduleDefinitionBeingEdited,
            moduleDefinitionBeingEditedIndex
        });
    }
    onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
        this.confirmAction(() => {
            this._onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex);
        });
    }
    onChange(key, event) {
        const { props, state } = this;
        let value = typeof event === "object" ? event.target.value : event;

        let { moduleDefinitionBeingEdited, error } = state;

        moduleDefinitionBeingEdited[key] = value;
        if (value === "" && (key === "friendlyName" || key === "name")) {
            error[key] = true;
        } else {
            error[key] = false;
        }

        this.setState({
            moduleDefinitionBeingEdited,
            error
        });

        if (!props.formIsDirty) {
            props.dispatch(ModuleDefinitionActions.setFormDirt(true));
        }
    }
    onSave() {
        const { props, state } = this;
        let { triedToSave, error } = state;
        triedToSave = true;
        this.setState({
            triedToSave
        });
        let errorCount = 0;
        Object.keys(error).forEach((key) => {
            if (error[key]) {
                errorCount++;
            }
        });

        if (errorCount > 0) {
            return;
        }

        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        let moduleDefinitions = extensionBeingUpdated.moduleDefinitions.value;
        if (state.moduleDefinitionBeingEditedIndex > -1) {
            moduleDefinitions[state.moduleDefinitionBeingEditedIndex] = state.moduleDefinitionBeingEdited;
        } else {
            moduleDefinitions.push(state.moduleDefinitionBeingEdited);
        }

        state.moduleDefinitionBeingEdited.desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        let actions = { savedefinition: JSON.stringify(state.moduleDefinitionBeingEdited) };

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
            props.dispatch(ModuleDefinitionActions.setFormDirt(false, () => {
                this.exitEditMode();
            }));
        }));
    }
    onControlSave() {
        const { props, state } = this;
        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        state.moduleDefinitionBeingEdited.desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        let actions = { savedefinition: JSON.stringify(state.moduleDefinitionBeingEdited) };

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
            props.dispatch(ModuleDefinitionActions.setFormDirt(false, () => {
                this.exitEditMode();
            }));
        }));
    }
    onDelete(definitionId) {
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;

            let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));

            let actions = { deletedefinition: definitionId.toString() };

            props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
                utils.utilities.notify(Localization.get("UpdateComplete"));
            }));
        });
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const moduleDefinitions = props.extensionBeingEdited.moduleDefinitions.value.map((moduleDefinition, index) => {

            return <ModuleDefinitionRow
                moduleDefinition={moduleDefinition}
                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited}
                extensionBeingEdited={props.extensionBeingEdited}
                extensionBeingEditedIndex={props.extensionBeingEditedIndex}
                onChange={this.onChange.bind(this)}
                onSave={this.onControlSave.bind(this)}
                error={state.error}
                triedToSave={state.triedToSave}
                controlFormIsDirty={props.controlFormIsDirty}
                onDelete={this.onDelete.bind(this, moduleDefinition.id, index)}
                isEditMode={state.moduleDefinitionBeingEditedIndex === index}
                onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                onEdit={this.onEditModuleDefinition.bind(this, Object.assign({}, moduleDefinition), index)}
                key={index} />;
        });

        const isAddMode = state.editMode && state.moduleDefinitionBeingEditedIndex === -1;

        return (
            <GridCell className="module-definitions">
                <GridCell className="header-container">
                    <h3 className="box-title">{Localization.get("EditModule_ModuleDefinitions.Header")}</h3>
                    <a className={"add-button" + (isAddMode ? " add-active" : "")}
                        onClick={this.onEditModuleDefinition.bind(this, this.getNewModuleDefinition(), -1)}>
                        <span dangerouslySetInnerHTML={{ __html: AddIcon }} className={isAddMode ? "svg-active" : ""}></span> {Localization.get("EditModule_Add.Button")}
                    </a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-definitions-table">
                    <Collapse isOpened={isAddMode} fixedHeight={300} style={{ float: "left" }}>
                        <GridCell className={"add-module-definition-box" + (isAddMode ? " row-opened" : "")}>
                            <DefinitionFields
                                onChange={this.onChange.bind(this)}
                                onControlSave={this.onSave.bind(this)}
                                error={state.error}
                                triedToSave={state.triedToSave}
                                isEditMode={false}
                                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited} />
                            <GridCell className="modal-footer">
                                <Button type="secondary" onClick={this.exitEditMode.bind(this)}>{Localization.get("ModuleDefinitions_Cancel.Button")}</Button>
                                <Button type="primary" onClick={this.onSave.bind(this)}>{Localization.get("ModuleDefinitions_Save.Button")}</Button>
                            </GridCell>
                        </GridCell>
                    </Collapse>
                    {moduleDefinitions}
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

ModuleDefinitions.PropTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty
    };
}

export default connect(mapStateToProps)(ModuleDefinitions);
