import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import DropdownWithError from "dnn-dropdown-with-error";
import GridCell from "dnn-grid-cell";
import { FolderActions, ExtensionActions } from "actions";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import FromControl from "./FromControl";
import FromManifest from "./FromManifest";
import FromNew from "./FromNew";
import utilities from "utils";
import Button from "dnn-button";
import styles from "./style.less";

const inputStyle = { width: "100%" };

const newModuleTypes = [{
    label: "New",
    value: 0
},
{
    label: "Control",
    value: 1
}, {
    label: "Manifest",
    value: 2
}];

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

    onSelectNewModuleType(option) {
        const { props } = this;
        props.dispatch(FolderActions.getOwnerFolders());
        props.dispatch(FolderActions.getModuleFolders(""));
        this.setState({
            selectedType: option.value
        });
    }

    getCreateButtonActive() {
        return this.state.selectedType !== 0 && this.state.selectedType !== 1 && this.state.selectedType !== 2;
    }

    onAddNewFolder(value, type, callback) {
        const { props } = this;
        let payload = {
            moduleFolder: "",
            ownerFolder: ""
        };
        payload[type] = value;

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
            props.onCancel();
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
                    onCancel={props.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)} />;
            case 1:
                return <FromControl
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={props.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)} />;
            case 2:
                return <FromManifest
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={props.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)} />;
            default:
                return <div></div>;
        }
    }

    render() {
        const {props} = this;
        return (
            <div className={styles.newModuleModal}>
                <SocialPanelHeader title="Create New Module" />
                <SocialPanelBody>
                    <GridCell className="new-module-box extension-form">
                        <GridCell columnSize={100} style={{ marginBottom: 15, padding: 0 }}>
                            <GridCell>
                                <DropdownWithError
                                    className="create-new-module-dropdown"
                                    options={newModuleTypes}
                                    tooltipMessage="Hey"
                                    value={this.state.selectedType}
                                    onSelect={this.onSelectNewModuleType.bind(this)}
                                    label="Create New Module From:"
                                    style={inputStyle}
                                    />
                            </GridCell>
                        </GridCell>
                        <GridCell style={{ padding: 0 }}>
                            {this.getCreateUI(this.state.selectedType)}
                        </GridCell>
                    </GridCell>
                </SocialPanelBody>
            </div>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

NewModuleModal.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func
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