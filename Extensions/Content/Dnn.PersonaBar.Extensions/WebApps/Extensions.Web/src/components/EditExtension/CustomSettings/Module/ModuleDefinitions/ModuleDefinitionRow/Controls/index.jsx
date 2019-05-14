import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, SvgIcons, Collapsible } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import ControlRow from "./ControlRow";
import utilities from "utils";
import { ModuleDefinitionActions, ExtensionActions } from "actions";
import ControlFields from "./ControlFields";
import utils from "utils";
import "./style.less";

function getSourceFolder(str) {
    return str.substr(0, str.lastIndexOf("/"));
}
function getFileName(str) {
    return str.substr(str.lastIndexOf("/") + 1, str.length - 1);
}
class Controls extends Component {
    constructor() {
        super();
        this.state = {
            editMode: false,
            controlBeingEdited: {},
            controlBeingEditedIndex: -1,
            error: {
                source: true
            },
            selectedSourceFolder: "Admin/Skins/"
        };
    }
    getNewControlKeys() {
        const { props } = this;
        return {
            id: -1,
            definitionId: props.moduleDefinitionId,
            key: "",
            title: "",
            source: "Admin/Skins/",
            type: -1,
            order: -1,
            icon: "",
            helpUrl: "",
            supportPopups: true,
            supportPartialRendering: true
        };
    }
    confirmAction(callback) {
        const { props } = this;
        if (props.controlFormIsDirty) {
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setControlFormDirt(false));
            });
        } else {
            callback();
        }
    }
    onChange(key, event) {
        const { state, props } = this;
        let value = typeof event === "object" ? event.target.value : event;
        let {controlBeingEdited, error } = state;

        controlBeingEdited[key] = value;

        if (value === "<None Specified>" && key === "source") {
            error[key] = true;
        } else {
            error[key] = false;
        }

        this.setState({
            controlBeingEdited,
            error
        });

        if (!props.controlFormIsDirty) {
            props.dispatch(ModuleDefinitionActions.setControlFormDirt(true));
        }
        if (!props.formIsDirty) {
            props.dispatch(ModuleDefinitionActions.setFormDirt(true));
        }
    }
    onDelete(controlId) {
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;

            let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));

            let actions = {deletemodulecontrol: controlId.toString()};

            props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex,  () => {
                utils.utilities.notify(Localization.get("UpdateComplete"));
                props.onControlSave();
            }));
        });
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
        let controls = JSON.parse(JSON.stringify(props.moduleControls));
        if (state.controlBeingEdited.id > -1) {
            controls[state.controlBeingEditedIndex] = state.controlBeingEdited;
        } else {
            controls.push(state.controlBeingEdited);
        }

        state.controlBeingEdited.definitionId = props.moduleDefinitionId;

        let actions = {savemodulecontrol: JSON.stringify(state.controlBeingEdited)};

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex,  () => {
            utils.utilities.notify(Localization.get("UpdateComplete"));
            props.onControlSave();
            props.dispatch(ModuleDefinitionActions.setControlFormDirt(false, () => {
                this.exitEditMode();
            }));
        }));
    }
    onEdit(controlBeingEdited, controlBeingEditedIndex) {
        this.confirmAction(() => {
            const { props } = this;
            const sourceFolder = getSourceFolder(controlBeingEdited.source) || "Admin/Skins/";
            props.dispatch(ModuleDefinitionActions.getSourceFolders());
            props.dispatch(ModuleDefinitionActions.getSourceFiles(sourceFolder, () => {
                props.dispatch(ModuleDefinitionActions.getControlIcons(controlBeingEdited.source, () => {
                    this.setState({
                        editMode: true,
                        controlBeingEdited,
                        controlBeingEditedIndex,
                        selectedSourceFolder: sourceFolder,
                        error: {
                            source: getFileName(controlBeingEdited.source) === ""
                        }
                    });
                }));
            }));
        });
    }

    exitEditMode() {
        this.confirmAction(() => {
            this.setState({
                controlBeingEdited: this.getNewControlKeys(),
                controlBeingEditedIndex: null,
                editMode: false,
                triedToSave: false
            });
        });
    }

    onSelectSourceFolder(option) {
        const { props } = this;
        this.setState({
            selectedSourceFolder: option.value
        });
        props.dispatch(ModuleDefinitionActions.getSourceFiles(option.value, () => {
            if (props.sourceFolders.length > 0) {
                props.dispatch(ModuleDefinitionActions.getControlIcons(option.value), () => {
                    this.setState({
                        selectedSourceFolder: option.value
                    });
                });
            }
        }));
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        const moduleControls = props.moduleControls && props.moduleControls.map((moduleControl, index) => {
            return <ControlRow
                moduleControl={moduleControl}
                controlBeingEdited={state.controlBeingEdited}
                onChange={this.onChange.bind(this)}
                onSave={this.onSave.bind(this)}
                onDelete={this.onDelete.bind(this, moduleControl.id, index)}
                sourceFolders={props.sourceFolders}
                icons={props.icons}
                error={state.error}
                triedToSave={state.triedToSave}
                sourceFiles={props.sourceFiles}
                onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                selectedSourceFolder={state.selectedSourceFolder}
                isEditMode={state.controlBeingEditedIndex === index}
                onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                onEdit={this.onEdit.bind(this, Object.assign({}, moduleControl), index)}
                key={index} />;
        });
        const isAddMode = state.editMode && state.controlBeingEditedIndex === -1;
        return (
            <GridCell className="module-controls">
                <GridCell className="header-container">
                    <h3 className="box-title">{Localization.get("ModuleDefinitions_ModuleControls.Header")}</h3>
                    <a className={"add-button" + (isAddMode ? " add-active" : "")} onClick={this.onEdit.bind(this, this.getNewControlKeys(), -1)}>
                        <span dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }} ></span> {Localization.get("EditModule_Add.Button")}</a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-controls-table">
                    <Collapsible isOpened={isAddMode} style={{ float: "left" }} className={"add-control-box" + (isAddMode ? " row-opened": "")}>
                        <ControlFields
                            controlBeingEdited={state.controlBeingEdited}
                            onChange={this.onChange.bind(this)}
                            onSave={this.onSave.bind(this)}
                            onDelete={this.onDelete.bind(this)}
                            error={state.error}
                            triedToSave={state.triedToSave}
                            sourceFolders={props.sourceFolders}
                            icons={props.icons}
                            sourceFiles={props.sourceFiles}
                            onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                            selectedSourceFolder={state.selectedSourceFolder}
                            onCancel={this.exitEditMode.bind(this)} /* Set definition being edited as null. */ />
                    </Collapsible>
                    <GridCell columnSize={15} className="module-control-title-header">
                        {Localization.get("AddModuleControl_Title.Label")}
                    </GridCell>
                    <GridCell columnSize={85} className="module-control-source-header">
                        {Localization.get("AddModuleControl_Source.Label")}
                    </GridCell>
                    {moduleControls}
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Controls.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        sourceFolders: state.moduleDefinition.sourceFolders,
        sourceFiles: state.moduleDefinition.sourceFiles,
        formIsDirty: state.moduleDefinition.formIsDirty,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty,
        icons: state.moduleDefinition.icons
    };
}

export default connect(mapStateToProps)(Controls);
