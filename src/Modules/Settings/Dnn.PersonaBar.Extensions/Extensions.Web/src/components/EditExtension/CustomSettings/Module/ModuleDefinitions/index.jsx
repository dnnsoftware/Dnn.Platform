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
import { ModuleDefinitionActions } from "actions";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class ModuleDefinitions extends Component {
    constructor() {
        super();
        this.state = {
            addDefinitionOpened: false
        };
    }
    toggleAddBox() {
        this.setState({
            addDefinitionOpened: !this.state.addDefinitionOpened
        });
    }
    onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
        const { props } = this;

        props.dispatch(ModuleDefinitionActions.editModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex));
        this.setState({});
    }
    clearModuleDefinitionBeingEdited() {
        const { props } = this;

        props.dispatch(ModuleDefinitionActions.clearModuleDefinitionBeingEdited());
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const moduleDefinitions = props.moduleDefinitions.map((moduleDefinition, index) => {
            return <ModuleDefinitionRow
                moduleDefinition={moduleDefinition}
                isEditMode={props.moduleDefinitionBeingEditedIndex === index}
                onCancel={this.clearModuleDefinitionBeingEdited.bind(this)} // Set definition being edited as null.
                onEdit={this.onEditModuleDefinition.bind(this, moduleDefinition, index)} />;
        });

        return (
            <GridCell className="module-definitions">
                <GridCell className="header-container">
                    <h3 className="box-title">Module Definitions</h3>
                    {!state.addDefinitionOpened && <a className="add-button" onClick={this.toggleAddBox.bind(this)}>
                        <span dangerouslySetInnerHTML={{ __html: AddIcon }}></span> Add</a>
                    }
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-definitions-table">
                    <Collapse isOpened={state.addDefinitionOpened} fixedHeight={300} style={{ float: "left" }}>
                        <GridCell className="add-module-definition-box">
                            <GridSystem>
                                <div>
                                    <SingleLineInputWithError
                                        label="Definition Name"
                                        tooltipMessage={"Placeholder"} />
                                    <SingleLineInputWithError
                                        label="Default Cache Time"
                                        tooltipMessage={"Placeholder"} />
                                </div>
                                <div>
                                    <SingleLineInputWithError
                                        label="Friendly Name"
                                        tooltipMessage={"Placeholder"} />
                                </div>
                            </GridSystem>
                            <GridCell className="modal-footer">
                                <Button type="secondary" onClick={this.toggleAddBox.bind(this)}>Cancel</Button>
                                <Button type="primary">Save</Button>
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
        moduleDefinitionBeingEditedIndex: state.moduleDefinition.moduleDefinitionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(ModuleDefinitions);