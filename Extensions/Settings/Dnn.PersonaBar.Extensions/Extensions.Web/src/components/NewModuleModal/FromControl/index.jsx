import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, GridSystem, SingleLineInputWithError, MultiLineInputWithError, DropdownWithError, Switch, Button } from "@dnnsoftware/dnn-react-common";
import FolderDropdown from "../common/FolderDropdown";
import Localization from "localization";
import { validationMapNewModule, valueMapNewModule } from "../common/helperFunctions";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function getValidateRequired(key) {
    switch (key) {
        case "moduleFolder":
        case "fileName":
        case "moduleName":
            return true;
        default:
            return false;
    }
}


const emptyNewModule = {
    type: 1,
    moduleFolder: "",
    ownerFolder: "",
    addPage: true,
    fileName: "",
    moduleName: "",
    description: ""
};

class FromControl extends Component {
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
    UNSAFE_componentWillReceiveProps(props) {
        if (props.moduleFiles.length > 0) {
            let { newModule } = this.state;
            newModule.fileName.value = props.moduleFiles[0].value;
            newModule.fileName.error = false;

            this.setState({
                newModule
            });
        }
    }

    onFolderSelect(key, option) {
        const { props, state } = this;
        let {newModule, triedToSave} = this.state;
        if (key === "ownerFolder") {
            newModule.fileName.value = "";
            newModule.fileName.error = true;
            newModule.moduleFolder.value = "";
            newModule.moduleFolder.error = true;
            triedToSave = false;
            props.onSelectOwnerFolder(option.value);
        }
        if (key === "moduleFolder" && option.value !== "") {
            newModule.fileName.value = "";
            newModule.fileName.error = true;
            triedToSave = false;
            props.onSelectModuleFolder({
                ownerFolder: state.newModule.ownerFolder.value,
                moduleFolder: option.value,
                type: 0
            });
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
            newModule.moduleFolder.value = "";
            newModule.moduleFolder.error = true;
            newModule.fileName.value = "";
            newModule.fileName.error = true;
            this.setState({
                triedToSave: false
            });
            this.props.onSelectOwnerFolder(data.ownerFolder);
        }
        if (type === "moduleFolder") {
            newModule.moduleFolder.value = data.moduleFolder;
            newModule.moduleFolder.error = false;
            newModule.fileName.value = "";
            newModule.fileName.error = true;
            this.setState({
                triedToSave: false
            });
            this.props.onSelectModuleFolder({
                ownerFolder: newModule.ownerFolder.value,
                moduleFolder: data.moduleFolder,
                type: 0
            });
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
            <GridCell className={styles.fromControl + " extension-form"} style={{ padding: 0 }}>
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
                        <DropdownWithError
                            label={Localization.get("NewModule_Resource.Label")}
                            tooltipMessage={Localization.get("NewModule_Resource.HelpText")}
                            style={inputStyle}
                            options={props.moduleFiles}
                            onSelect={this.onFolderSelect.bind(this, "fileName")}
                            value={state.newModule.fileName.value}
                            error={state.newModule.fileName.error && state.triedToSave} />
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

FromControl.propTypes = {
    onCancel: PropTypes.func,
    ownerFolders: PropTypes.array,
    moduleFolders: PropTypes.array,
    moduleFiles: PropTypes.array,
    onCreateNewModule: PropTypes.func,
    onSelectOwnerFolder: PropTypes.func,
    onSelectModuleFolder: PropTypes.func,
    onAddNewFolder: PropTypes.func,
    retrieveOwnerAndModuleFolders: PropTypes.func
};

export default FromControl;