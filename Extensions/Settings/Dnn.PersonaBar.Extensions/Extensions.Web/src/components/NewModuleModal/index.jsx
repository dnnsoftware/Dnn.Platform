import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DropdownWithError, GridCell, PersonaBarPageBody, PersonaBarPageHeader, Button } from "@dnnsoftware/dnn-react-common";
import { FolderActions, ExtensionActions, VisiblePanelActions } from "actions";
import FromControl from "./FromControl";
import FromManifest from "./FromManifest";
import FromNew from "./FromNew";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };

const newModuleTypes = [
    {
        label: "New",
        value: 0
    },
    {
        label: "Control",
        value: 1
    },
    {
        label: "Manifest",
        value: 2
    }
];

class NewModuleModal extends Component {
    constructor() {
        super();
        this.state = {
            selectedType: ""
        };
    }

    onSelectOwnerFolder(ownerFolder) {
        const { props } = this;
        props.dispatch(FolderActions.getModuleFolders(ownerFolder));
    }

    onSelectModuleFolder(parameters) {
        const { props } = this;
        props.dispatch(FolderActions.getModuleFiles(parameters));
    }

    retrieveOwnerAndModuleFolders() {
        this.props.dispatch(FolderActions.getOwnerFolders());
        this.props.dispatch(FolderActions.getModuleFolders(""));
    }

    onSelectNewModuleType(option) {
        this.setState({
            selectedType: option.value
        });
    }

    getCreateButtonActive() {
        return this.state.selectedType !== 0 && this.state.selectedType !== 1 && this.state.selectedType !== 2;
    }

    onAddNewFolder(payload, type, callback) {
        const { props } = this;

        props.dispatch(FolderActions.createFolder(payload, type, callback));
    }

    onCreateNewModule(payload) {
        const { props } = this;
        const shouldAppend = props.selectedInstalledPackageType === "Module";
        props.dispatch(ExtensionActions.createNewModule(payload, shouldAppend, (data) => {
            if (data.NewPageUrl) {
                window.open(data.NewPageUrl);
            }
            if (!shouldAppend) {
                props.dispatch(ExtensionActions.getInstalledPackages("Module"));
            }
            this.onCancel();
        }));
    }

    getCreateUI(selectedType) {
        const { props } = this;
        const ownerFolders = props.ownerFolders.map(folder => {
            return {
                value: folder,
                label: folder.split(/(?=[A-Z])/).join(" ")
            };
        });
        const moduleFolders = props.moduleFolders.map(folder => {
            return {
                value: folder,
                label: folder.split(/(?=[A-Z])/).join(" ")
            };
        });
        const moduleFiles = props.moduleFiles.map(file => {
            return {
                value: file,
                label: file
            };
        });
        switch (selectedType) {
            case 0:
                return <FromNew
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            case 1:
                return <FromControl
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            case 2:
                return <FromManifest
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            default:
                return <div></div>;
        }
    }

    onCancel() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(0));
    }

    render() {
        return (
            <div className={styles.newModuleModal}>
                <PersonaBarPageHeader title={Localization.get("CreateModule.Action")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToExtensions"),
                    onClick: this.onCancel.bind(this)
                }}>
                    <GridCell className="new-module-box extension-form">
                        <GridCell columnSize={100} style={{ marginBottom: 15, padding: 0 }}>
                            <GridCell className="new-module-dropdown-container">
                                <DropdownWithError
                                    className="create-new-module-dropdown"
                                    options={newModuleTypes}
                                    tooltipMessage={Localization.get("CreateNewModule.HelpText")}
                                    value={this.state.selectedType}
                                    onSelect={this.onSelectNewModuleType.bind(this)}
                                    label={Localization.get("CreateNewModuleFrom.Label")}
                                    style={inputStyle} />
                                {this.state.selectedType === "" &&
                                    <Button type="secondary" onClick={this.onCancel.bind(this)}>{Localization.get("NewModule_Cancel.Button")}</Button>
                                }
                            </GridCell>
                        </GridCell>
                        <GridCell style={{ padding: 0 }}>
                            {this.getCreateUI(this.state.selectedType)}
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
            </div>
        );
    }
}

NewModuleModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    selectedInstalledPackageType: PropTypes.string,
    ownerFolders: PropTypes.array,
    moduleFolders: PropTypes.array,
    moduleFiles: PropTypes.array
};

function mapStateToProps(state) {
    return {
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        ownerFolders: state.folder.ownerFolders,
        moduleFolders: state.folder.moduleFolders,
        moduleFiles: state.folder.moduleFiles
    };
}

export default connect(mapStateToProps)(NewModuleModal);
