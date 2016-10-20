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


function getSourceFolder(str) {
    return str.substr(0, str.lastIndexOf("/"));
}
class Controls extends Component {
    constructor() {
        super();
        this.state = {
            editMode: false,
            controlBeingEdited: {},
            controlBeingEditedIndex: -1,
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
    onChange(key, event) {
        const { state } = this;
        let value = typeof event === "object" ? event.target.value : event;
        let {controlBeingEdited } = state;

        controlBeingEdited[key] = value;

        this.setState({
            controlBeingEdited
        });

    }
    onDelete() {

    }
    onSave() {
        const { props, state } = this;
        props.dispatch(ModuleDefinitionActions.addOrUpdateModuleControl(state.controlBeingEdited, () => {
            let _controls = JSON.parse(JSON.stringify(props.moduleControls));
            if (state.controlBeingEdited.id > -1) {
                _controls[state.controlBeingEditedIndex] = state.controlBeingEdited;
            } else {
                _controls.push(state.controlBeingEdited);
            }
            props.onChange({ target: { value: _controls } });
        }));
    }
    onEdit(controlBeingEdited, controlBeingEditedIndex) {
        const { props } = this;
        const sourceFolder = getSourceFolder(controlBeingEdited.source) || "Admin/Skins/";
        props.dispatch(ModuleDefinitionActions.getSourceFolders());

        props.dispatch(ModuleDefinitionActions.getSourceFiles(sourceFolder, () => {
            props.dispatch(ModuleDefinitionActions.getControlIcons(controlBeingEdited.source, () => {
                this.setState({
                    editMode: true,
                    controlBeingEdited,
                    controlBeingEditedIndex,
                    selectedSourceFolder: sourceFolder
                });
            }));
        }));
    }
    onCancel() {
        this.setState({
            controlBeingEdited: this.getNewControlKeys(),
            controlBeingEditedIndex: null,
            editMode: false
        });
    }

    onSelectSourceFolder(option) {
        const { props } = this;
        this.setState({
            selectedSourceFolder: option.value
        });
        props.dispatch(ModuleDefinitionActions.getSourceFiles(option.value, () => {
            if (props.sourceFolders.length > 0) {
                props.dispatch(ModuleDefinitionActions.getControlIcons(props.sourceFiles[0].Value), () => {
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
                onDelete={this.onDelete.bind(this)}
                sourceFolders={props.sourceFolders}
                icons={props.icons}
                sourceFiles={props.sourceFiles}
                onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                selectedSourceFolder={state.selectedSourceFolder}
                isEditMode={state.controlBeingEditedIndex === index}
                onCancel={this.onCancel.bind(this)} // Set definition being edited as null.
                onEdit={this.onEdit.bind(this, Object.assign({}, moduleControl), index)} />;
        });

        return (
            <GridCell className="module-controls">
                <GridCell className="header-container">
                    <h3 className="box-title">Module Controls</h3>
                    <a className="add-button" onClick={this.onEdit.bind(this, this.getNewControlKeys(), -1)}>
                        <span dangerouslySetInnerHTML={{ __html: AddIcon }}></span> Add</a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-controls-table">
                    <Collapse isOpened={state.editMode && state.controlBeingEditedIndex === -1} style={{ float: "left" }}>
                        <ControlFields
                            controlBeingEdited={state.controlBeingEdited}
                            onChange={this.onChange.bind(this)}
                            onSave={this.onSave.bind(this)}
                            onDelete={this.onDelete.bind(this)}
                            sourceFolders={props.sourceFolders}
                            icons={props.icons}
                            sourceFiles={props.sourceFiles}
                            onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                            selectedSourceFolder={state.selectedSourceFolder}
                            onCancel={this.onCancel.bind(this)} // Set definition being edited as null.
                            />
                    </Collapse>
                    <GridCell columnSize={15} className="module-control-title-header">
                        Title
                    </GridCell>
                    <GridCell columnSize={85} className="module-control-source-header">
                        Source
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
        icons: state.moduleDefinition.icons
    };
}

export default connect(mapStateToProps)(Controls);