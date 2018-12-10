import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
import FolderDropdown from "../common/FolderDropdown";
import { validationMapNewModule, valueMapNewModule } from "../common/helperFunctions";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function getValidateRequired(key) {
    switch (key) {
        case "moduleFolder":
        case "fileName":
        case "moduleName":
        case "language":
            return true;
        default:
            return false;
    }
}

const emptyNewModule = {
    type: 0,
    moduleFolder: "",
    ownerFolder: "",
    addPage: true,
    fileName: "",
    moduleName: "",
    description: "",
    language: "C#"
};

class FromNew extends Component {
    constructor() {
        super();
        this.state = {
            newModule: validationMapNewModule(emptyNewModule, getValidateRequired),
            triedToSave: false
        };
    }

    UNSAFE_componentWillMount() {
        this.props.retrieveOwnerAndModuleFolders();
    }

    onFolderSelect(key, option) {
        const { props } = this;
        let {newModule, triedToSave} = this.state;
        if (key === "ownerFolder") {
            newModule.moduleFolder.value = "";
            newModule.moduleFolder.error = true;
            triedToSave = false;
            props.onSelectOwnerFolder(option.value);
        }
        if (newModule[key].required) {
            newModule[key].error = !option.value;
        }
        newModule[key].value = option.value;
        this.setState({
            newModule,
            triedToSave
        });
    }

    onChange(key, event) {
        let value = typeof event === "object" ? event.target.value : event;
        let {newModule} = this.state;
        newModule[key].value = value;
        if (newModule[key].required) {
            newModule[key].error = !value;
        }
        this.setState({
            newModule
        });
    }

    validateFields() {
        let {triedToSave, newModule} = this.state;
        let errorCount = 0;
        Object.keys(newModule).forEach((key) => {
            if (newModule[key].error) {
                errorCount++;
            }
        });
        triedToSave = true;
        this.setState({
            newModule,
            triedToSave
        });
        return errorCount === 0;
    }

    onCreateNewModule() {
        let {state, props} = this;
        let {triedToSave} = state;
        triedToSave = true;
        if (!this.validateFields()) {
            return;
        }
        this.setState({
            triedToSave
        });
        props.onCreateNewModule(valueMapNewModule(state.newModule));
    }
    onAddedNewFolder(data, type) {
        let {newModule} = this.state;
        if (type === "ownerFolder") {
            newModule.ownerFolder.value = data.ownerFolder;
            this.props.onSelectOwnerFolder(data.ownerFolder);
            if (newModule.moduleFolder.value !== "") {
                newModule.moduleFolder.value = "";
                newModule.moduleFolder.error = true;
            }
            this.setState({
                triedToSave: false
            });
        }
        if (type === "moduleFolder") {
            newModule.moduleFolder.value = data.moduleFolder;
            newModule.moduleFolder.error = false;
        }
        this.setState({
            newModule
        });
    }

    onAddNewFolder(value, type, callback) {
        const { newModule } = this.state;
        let payload = {
            moduleFolder: type === "ownerFolder" ? "" : newModule.moduleFolder.value,
            ownerFolder: newModule.ownerFolder.value
        };
        payload[type] = value;

        this.props.onAddNewFolder(payload, type, callback);
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className={styles.fromNew + " extension-form"} style={{ padding: 0 }}>
                <GridSystem className="with-right-border top-half">
                    <div style={{ paddingRight: 15 }}>
                        <FolderDropdown
                            folders={props.ownerFolders}
                            label={Localization.get("NewModule_OwnerFolder.Label")}
                            type="ownerFolder"
                            tooltipMessage={Localization.get("NewModule_OwnerFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "ownerFolder")}
                            value={state.newModule.ownerFolder.value}
                            onAddNewFolder={this.onAddNewFolder.bind(this)}
                            onAddedNewFolder={this.onAddedNewFolder.bind(this)} />
                        <FolderDropdown
                            folders={props.moduleFolders}
                            label={Localization.get("NewModule_ModuleFolder.Label")}
                            type="moduleFolder"
                            tooltipMessage={Localization.get("NewModule_ModuleFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "moduleFolder")}
                            value={state.newModule.moduleFolder.value}
                            onAddNewFolder={this.onAddNewFolder.bind(this)}
                            error={state.newModule.moduleFolder.error && state.triedToSave}
                            onAddedNewFolder={this.onAddedNewFolder.bind(this)} />
                        <SingleLineInputWithError
                            label={Localization.get("NewModule_FileName.Label")}
                            tooltipMessage={Localization.get("NewModule_FileName.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            onChange={this.onChange.bind(this, "fileName")}
                            value={state.newModule.fileName.value}
                            error={state.newModule.fileName.error && state.triedToSave} />
                        <RadioButtons 
                            options={[
                                {
                                    label: "Visual Basic",
                                    value: "Visual Basic"
                                },
                                {
                                    label: "C Sharp",
                                    value: "C#"
                                }
                            ]}
                            label={Localization.get("NewModule_Language.Label")}
                            value={state.newModule.language.value}
                            buttonGroup="language"
                            tooltipMessage={Localization.get("NewModule_Language.HelpText")}
                            onChange={this.onChange.bind(this, "language")}
                            float="right" />
                    </div>
                    <div style={{ paddingLeft: 15 }}>
                        <SingleLineInputWithError
                            label={Localization.get("NewModule_ModuleName.Label")}
                            tooltipMessage={Localization.get("NewModule_ModuleName.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={state.newModule.moduleName.value}
                            onChange={this.onChange.bind(this, "moduleName")}
                            error={state.newModule.moduleName.error && state.triedToSave} />
                        <MultiLineInputWithError
                            label={Localization.get("NewModule_Description.Label")}
                            tooltipMessage={Localization.get("NewModule_Description.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32, height: 126 }}
                            onChange={this.onChange.bind(this, "description")}
                            value={state.newModule.description.value} />
                        <Switch value={state.newModule.addPage.value}
                            tooltipMessage={Localization.get("NewModule_AddTestPage.HelpText")}
                            label={Localization.get("NewModule_AddTestPage.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            onChange={this.onChange.bind(this, "addPage")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("NewModule_Cancel.Button")}</Button>
                    <Button type="primary"
                        onClick={this.onCreateNewModule.bind(this)}>{Localization.get("NewModule_Create.Button")}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

FromNew.propTypes = {
    onCancel: PropTypes.func,
    ownerFolders: PropTypes.array,
    onCreateNewModule: PropTypes.func,
    onSelectOwnerFolder: PropTypes.func,
    onSelectModuleFolder: PropTypes.func,
    onAddNewFolder: PropTypes.func,
    retrieveOwnerAndModuleFolders: PropTypes.func
};

export default FromNew;