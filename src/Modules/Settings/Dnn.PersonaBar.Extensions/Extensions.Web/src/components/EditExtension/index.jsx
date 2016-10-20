import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { ExtensionActions, VisiblePanelActions, ModuleDefinitionActions } from "actions";
import Tabs from "dnn-tabs";
import License from "./License";
import ReleaseNotes from "./ReleaseNotes";
import PackageInformation from "./PackageInformation";
import CustomSettings from "./CustomSettings";
import Tooltip from "dnn-tooltip";
import utilities from "utils";
import styles from "./style.less";

class EditExtension extends Component {
    constructor() {
        super();
        this.state = {
            extensionBeingEdited: {
                type: "",
                name: "",
                description: "",
                friendlyName: "",
                version: "9.0.0",
                owner: "",
                url: "",
                organization: "",
                email: ""
            },
            selectedTabIndex: 0
        };
    }

    onVersionChange(index, option) {
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        if (_extensionBeingEdited.version && _extensionBeingEdited.version.value) {
            let versionArray = _extensionBeingEdited.version.value.split(".");
            versionArray[index] = option.value;
            _extensionBeingEdited.version.value = versionArray.join(".");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }

    onChange(key, event) {
        const { props } = this;
        const value = typeof event === "object" ? event.target.value : event;

        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        let field = _extensionBeingEdited[key];
        field.value = value;

        if (field.validateRequired && field.error) {
            field.error = false;
            this.toggleTriedToSave();
            this.toggleTabError(field.tabMapping, "remove");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }

    onAssignedPortalsChange(key, value, callback) {
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        _extensionBeingEdited[key].value = value;
        this.updateExtensionBeingEdited(_extensionBeingEdited, callback);
    }

    updateExtensionBeingEdited(extensionBeingEdited, callback) {
        const { props } = this;
        props.dispatch(ExtensionActions.updateExtensionBeingEdited(extensionBeingEdited, callback));
    }

    onSaveExtension() {
        const {props} = this;
        props.dispatch(ExtensionActions.updateExtension(props.extensionBeingEdited, props.extensionBeingEditedIndex));
        this.selectPanel(0);
    }

    toggleTriedToSave() {
        const {props} = this;
        props.dispatch(ExtensionActions.toggleTriedToSave());
    }

    toggleTabError(tabWithError, action) {
        const { props } = this;
        props.dispatch(ExtensionActions.toggleTabError(tabWithError, action));
    }

    validateFields() {
        const { props } = this;
        let errorCount = 0;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        Object.keys(_extensionBeingEdited).forEach((key) => {
            let field = _extensionBeingEdited[key];
            if (field.validateRequired && field.value === "") {
                field.error = true;
                errorCount++;
                this.toggleTabError(field.tabMapping);
            }
        });
        if (props.triedToSave === false) {
            this.toggleTriedToSave();
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
        return errorCount === 0;
    }

    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
    }

    getTabHeaders() {
        const { props } = this;
        return ["Package Information", "Extension Settings", "Site Settings", "License", "Release Notes"].map((tabHeader, index) => {
            const hasError = props.tabsWithError.indexOf(index) > -1;
            return <span>{tabHeader} <Tooltip type="error" rendered={hasError} messages={["This field has an error"]} /></span>;
        });
    }

    onSave() {
        const { props } = this;
        if (!this.validateFields()) {
            return;
        }
        this.onSaveExtension();
    }
    confirmAction(callback) {
        const { props } = this;
        if (props.moduleDefinitionFormIsDirty) {
            this.setState({});
            utilities.utilities.confirm("You have unsaved changes. Are you sure you want to proceed?", "Yes", "No", () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setFormDirt(false));
            });
        } else {
            callback();
        }
    }
    onTabSelect(index) {
        this.confirmAction(() => {
            this.setState({
                selectedTabIndex: index
            });
        });
    }

    render() {
        const {props, state} = this;
        const {extensionBeingEdited} = props;
        return (
            <GridCell className={styles.editExtension}>
                <SocialPanelHeader title={extensionBeingEdited.friendlyName.value + " Extension"} />
                <SocialPanelBody>
                    <Tabs
                        tabHeaders={this.getTabHeaders()}
                        onSelect={this.onTabSelect.bind(this)}
                        selectedIndex={state.selectedTabIndex}
                        type="primary">
                        <GridCell className="package-information-box extension-form">
                            <PackageInformation
                                onSave={this.onSave.bind(this)}
                                extensionBeingEdited={extensionBeingEdited}
                                onVersionChange={this.onVersionChange.bind(this)}
                                onCancel={this.selectPanel.bind(this, 0)}
                                validateFields={this.validateFields.bind(this)}
                                onChange={this.onChange.bind(this)}
                                updateExtensionBeingEdited={this.updateExtensionBeingEdited.bind(this)}
                                triedToSave={props.triedToSave}
                                toggleTriedToSave={this.toggleTriedToSave.bind(this)}
                                primaryButtonText="Update" />
                        </GridCell>
                        <GridCell className="extension-form">
                            <CustomSettings
                                type="Module"
                                primaryButtonText="Next"
                                onChange={this.onChange.bind(this)}
                                onCancel={this.selectPanel.bind(this, 0)}
                                onSave={this.onSave.bind(this)}
                                onAssignedPortalsChange={this.onAssignedPortalsChange.bind(this)}
                                />
                        </GridCell>
                        <GridCell>
                            Site Settings
                        </GridCell>
                        <License value={extensionBeingEdited.license.value}
                            onChange={this.onChange.bind(this)}
                            onCancel={this.selectPanel.bind(this, 0)}
                            onSave={this.onSave.bind(this)}
                            primaryButtonText="Update" />
                        <ReleaseNotes
                            value={extensionBeingEdited.releaseNotes.value}
                            onChange={this.onChange.bind(this)}
                            onCancel={this.selectPanel.bind(this, 0)}
                            onSave={this.onSave.bind(this)}
                            primaryButtonText="Update" />
                    </Tabs>
                </SocialPanelBody>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

EditExtension.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    disabled: PropTypes.func,
    packageBeingEditedSettings: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex,
        packageBeingEditedSettings: state.extension.packageBeingEditedSettings,
        moduleDefinitionFormIsDirty: state.moduleDefinition.formIsDirty,
        triedToSave: state.extension.triedToSave,
        tabsWithError: state.extension.tabsWithError
    };
}

export default connect(mapStateToProps)(EditExtension);