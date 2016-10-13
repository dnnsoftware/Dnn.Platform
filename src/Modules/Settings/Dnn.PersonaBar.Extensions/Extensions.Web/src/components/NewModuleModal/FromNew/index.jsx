import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
import FolderDropdown from "../common/FolderDropdown";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class FromNew extends Component {
    constructor() {
        super();
        this.state = {
            type: 0,
            moduleFolder: "",
            ownerFolder: "",
            addPage: true,
            fileName: "",
            moduleName: "",
            description: "",
            language: "C#"
        };
    }

    onFolderSelect(key, option) {
        const { props } = this;
        if (key === "ownerFolder") {
            props.onSelectOwnerFolder(option.value);
        }
        this.setState({
            [key]: option.value
        });
    }

    onChange(key, event) {
        let value = typeof event === "object" ? event.target.value : event;

        this.setState({
            [key]: value
        });
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className={styles.fromNew + " extension-form"} style={{ padding: 0 }}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <FolderDropdown
                            folders={props.ownerFolders}
                            label={Localization.get("NewModule_OwnerFolder.Label")}
                            type="ownerFolder"
                            tooltipMessage={Localization.get("NewModule_OwnerFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "ownerFolder")}
                            value={state.ownerFolder}
                            onAddNewFolder={props.onAddNewFolder.bind(this)} />
                        <FolderDropdown
                            folders={props.moduleFolders}
                            label={Localization.get("NewModule_ModuleFolder.Label")}
                            type="moduleFolder"
                            tooltipMessage={Localization.get("NewModule_ModuleFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "moduleFolder")}
                            value={state.moduleFolder}
                            enabled={state.ownerFolder !== ""}
                            onAddNewFolder={props.onAddNewFolder.bind(this)} />
                        <SingleLineInputWithError
                            label={Localization.get("NewModule_FileName.Label")}
                            tooltipMessage={Localization.get("NewModule_FileName.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            onChange={this.onChange.bind(this, "fileName")}
                            value={state.fileName}
                            />
                        <RadioButtons options={[
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
                            value={state.language}
                            buttonGroup="language"
                            tooltipMessage={Localization.get("NewModule_Language.HelpText")}
                            onChange={this.onChange.bind(this, "language")}
                            float="right" />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("NewModule_ModuleName.Label")}
                            tooltipMessage={Localization.get("NewModule_ModuleName.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={state.moduleName}
                            onChange={this.onChange.bind(this, "moduleName")}
                            />
                        <MultiLineInputWithError
                            label={Localization.get("NewModule_Description.Label")}
                            tooltipMessage={Localization.get("NewModule_Description.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32, height: 126 }}
                            onChange={this.onChange.bind(this, "description")}
                            value={state.description} />
                        <Switch value={state.addPage}
                            tooltipMessage={Localization.get("NewModule_AddTestPage.HelpText")}
                            label={Localization.get("NewModule_AddTestPage.Label")}
                            onChange={this.onChange.bind(this, "addPage")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("NewModule_Cancel.Button")}</Button>
                    <Button type="primary"
                        onClick={props.onCreateNewModule.bind(this, state)}>{Localization.get("NewModule_Create.Button")}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

FromNew.PropTypes = {
    onCancel: PropTypes.func,
    ownerFolders: PropTypes.array
};

export default FromNew;