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
import DefinitionFields from "./DefinitionFields";
import styles from "./style.less";


function removeRecordFromArray(arr, index) {
    return [...arr.slice(0, index), ...arr.slice(index + 1)];
}
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
    componentWillMount() {
        this.setState({
            moduleDefinitionBeingEdited: this.getNewModuleDefinition()
        });
    }
    confirmAction(callback) {
        const { props } = this;
        if (props.formIsDirty) {
            utilities.utilities.confirm("You have unsaved changes. Are you sure you want to proceed?", "Yes", "No", () => {
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
        console.log(this.state);
        if (errorCount > 0) {
            return;
        }

        props.dispatch(ModuleDefinitionActions.addOrUpdateModuleDefinition(state.moduleDefinitionBeingEdited, () => {
            let _moduleDefinitions = JSON.parse(JSON.stringify(props.moduleDefinitions));
            if (state.moduleDefinitionBeingEditedIndex > -1) {
                _moduleDefinitions[state.moduleDefinitionBeingEditedIndex] = state.moduleDefinitionBeingEdited;
            } else {
                _moduleDefinitions.push(state.moduleDefinitionBeingEdited);
            }
            props.onSave({ target: { value: _moduleDefinitions } });
            props.dispatch(ModuleDefinitionActions.setFormDirt(false, () => {
                this.exitEditMode();
            }));

        }));
    }
    onDelete(definitionId, index) {
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;
            props.dispatch(ModuleDefinitionActions.deleteModuleDefinition(definitionId, () => {
                props.onSave({ target: { value: removeRecordFromArray(props.moduleDefinitions, index) } });
            }));
        });
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        const moduleDefinitions = props.moduleDefinitions.map((moduleDefinition, index) => {
            return <ModuleDefinitionRow
                moduleDefinition={moduleDefinition}
                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited}
                onChange={this.onChange.bind(this)}
                onSave={this.onSave.bind(this)}
                error={state.error}
                triedToSave={state.triedToSave}
                controlFormIsDirty={props.controlFormIsDirty}
                onDelete={this.onDelete.bind(this, moduleDefinition.id, index)}
                isEditMode={state.moduleDefinitionBeingEditedIndex === index}
                onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                onEdit={this.onEditModuleDefinition.bind(this, Object.assign({}, moduleDefinition), index)} />;
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
                        <GridCell className="add-module-definition-box">
                            <DefinitionFields
                                onChange={this.onChange.bind(this)}
                                error={state.error}
                                triedToSave={state.triedToSave}
                                isEditMode={false}
                                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited}
                                />
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
        moduleDefinitionBeingEdited: state.moduleDefinition.moduleDefinitionBeingEdited,
        moduleDefinitionBeingEditedIndex: state.moduleDefinition.moduleDefinitionBeingEditedIndex,
        formIsDirty: state.moduleDefinition.formIsDirty,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty
    };
}

export default connect(mapStateToProps)(ModuleDefinitions);