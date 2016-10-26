import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Button from "dnn-button";
import Localization from "localization";
import { AddIcon } from "dnn-svg-icons";
import ControlRow from "./ControlRow";
import Collapse from "react-collapse";
import utilities from "utils";
import { ModuleDefinitionActions } from "actions";
import ControlFields from "./ControlFields";
import styles from "./style.less";

function removeRecordFromArray(arr, index) {
    return [...arr.slice(0, index), ...arr.slice(index + 1)];
}
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
            utilities.utilities.confirm("You have unsaved changes. Are you sure you want to proceed?", "Yes", "No", () => {
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
    onDelete(controlId, index) {
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;
            props.dispatch(ModuleDefinitionActions.deleteModuleControl(controlId, () => {
                props.onChange({ target: { value: removeRecordFromArray(props.moduleControls, index) } });
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
        props.dispatch(ModuleDefinitionActions.addOrUpdateModuleControl(state.controlBeingEdited, () => {
            let _controls = JSON.parse(JSON.stringify(props.moduleControls));
            if (state.controlBeingEdited.id > -1) {
                _controls[state.controlBeingEditedIndex] = state.controlBeingEdited;
            } else {
                _controls.push(state.controlBeingEdited);
            }
            props.onChange({ target: { value: _controls } });
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
                onEdit={this.onEdit.bind(this, Object.assign({}, moduleControl), index)} />;
        });
        const isAddMode = state.editMode && state.controlBeingEditedIndex === -1;
        return (
            <GridCell className="module-controls">
                <GridCell className="header-container">
                    <h3 className="box-title">{Localization.get("ModuleDefinitions_ModuleControls.Header")}</h3>
                    <a className={"add-button" + (isAddMode ? " add-active" : "")} onClick={this.onEdit.bind(this, this.getNewControlKeys(), -1)}>
                        <span dangerouslySetInnerHTML={{ __html: AddIcon }} ></span> {Localization.get("EditModule_Add.Button")}</a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-controls-table">
                    <Collapse isOpened={isAddMode} style={{ float: "left" }} className="add-control-box">
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
                            onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                            />
                    </Collapse>
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

Controls.PropTypes = {
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