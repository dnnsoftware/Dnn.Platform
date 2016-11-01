import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import DropdownWithError from "dnn-dropdown-with-error";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import Localization from "localization";
import Dropdown from "dnn-dropdown";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import { ExtensionActions, VisiblePanelActions } from "actions";
import Button from "dnn-button";
import CustomSettings from "../EditExtension/CustomSettings";
import BasicPackageInformation from "../common/BasicPackageInformation";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import styles from "./style.less";

const inputStyle = { width: "100%" };

function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}

function getDropdownOptions() {
    let options = [];
    for (let i = 0; i < 100; i++) {
        options.push({
            label: formatVersionNumber(i),
            value: i
        });
    }
    return options;
}

function deepCopy(object) {
    return JSON.parse(JSON.stringify(object));
}


class NewExtensionModal extends Component {
    constructor() {
        super();
        this.versionDropdownOptions = getDropdownOptions();
    }

    componentWillMount() {
        const { props } = this;
        if ((!props.moduleCategories || props.moduleCategories.length === 0)) {
            props.dispatch(ExtensionActions.getModuleCategories());
        }
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
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }
    onVersionChange(index, option) {
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        if (_extensionBeingEdited.version) {
            let versionArray = _extensionBeingEdited.version.value.split(".");
            versionArray[index] = option.value;
            _extensionBeingEdited.version.value = versionArray.join(".");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }
    onPackageTypeSelect(option) {
        let value = option.value;

        const { props } = this;
        let _extensionBeingEdited = deepCopy(props.extensionBeingEdited);

        _extensionBeingEdited.packageType.value = value;

        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }

    updateExtensionBeingEdited(extensionBeingEdited, callback) {
        const { props } = this;
        props.dispatch(ExtensionActions.updateExtensionBeingEdited(extensionBeingEdited, callback));
    }
    parseEditorActions(extension) {
        switch (extension.packageType.value.toLowerCase()) {
            case "module":
                return {
                    category: extension.category.value,
                    dependencies: extension.dependencies.value,
                    hostPermissions: extension.hostPermissions.value,
                    shareable: extension.shareable.value,
                    premiumModule: extension.premiumModule.value,
                    folderName: extension.folderName.value,
                    businessController: extension.businessController.value
                };
            default:
                return {};
        }
    }

    onSaveExtension() {
        const {props} = this;

        let editorActions = this.parseEditorActions(props.extensionBeingEdited);
        props.dispatch(ExtensionActions.createNewExtension(props.extensionBeingEdited, editorActions, props.extensionBeingEditedIndex));
        this.selectPanel(0);
    }
    toggleTriedToSave() {
        const {props} = this;
        props.dispatch(ExtensionActions.toggleTriedToSave());
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


    onSave(close) {
        const { props } = this;
        if (!this.validateFields()) {
            return;
        }

        this.onSaveExtension();
    }
    render() {
        const {props} = this;
        const {extensionBeingEdited} = props;
        const version = extensionBeingEdited.version.value ? extensionBeingEdited.version.value.split(".") : [0, 0, 0];
        return (
            <div className={styles.newExtensionModal}>
                <SocialPanelHeader title="Create New Extension" />
                <SocialPanelBody>
                    <GridCell className="new-extension-box extension-form">
                        <BasicPackageInformation
                            validationMapped={true}
                            installedPackageTypes={props.installedPackageTypes}
                            extensionData={extensionBeingEdited}
                            onChange={this.onChange.bind(this)}
                            triedToSave={props.triedToSave}
                            version={version}
                            onPackageTypeSelect={this.onPackageTypeSelect.bind(this)}
                            onVersionChange={this.onVersionChange.bind(this)}
                            isAddMode={true}
                            />
                        <GridCell><hr /></GridCell>
                        <GridCell className="box-title-container">
                            <h3 className="box-title">{Localization.get("EditExtension_OwnerDetails.Label")}</h3>
                        </GridCell>
                        <GridSystem className="with-right-border bottom-half">
                            <div>
                                <SingleLineInputWithError
                                    label="Owner"
                                    tooltipMessage={Localization.get("EditExtension_PackageOwner.HelpText")}
                                    style={inputStyle}
                                    value={extensionBeingEdited.owner.value}
                                    onChange={this.onChange.bind(this, "owner")} />
                                <SingleLineInputWithError
                                    label="Organization"
                                    tooltipMessage={Localization.get("EditExtension_PackageOrganization.HelpText")}
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 0 }}
                                    value={extensionBeingEdited.organization.value}
                                    onChange={this.onChange.bind(this, "organization")} />
                            </div>
                            <div>
                                <SingleLineInputWithError
                                    label="URL"
                                    tooltipMessage={Localization.get("EditExtension_PackageURL.HelpText")}
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 32 }}
                                    value={extensionBeingEdited.url.value}
                                    onChange={this.onChange.bind(this, "url")} />
                                <SingleLineInputWithError
                                    label="Email Address"
                                    tooltipMessage={Localization.get("EditExtension_PackageEmailAddress.HelpText")}
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 32 }}
                                    value={extensionBeingEdited.email.value}
                                    onChange={this.onChange.bind(this, "email")} />
                            </div>
                        </GridSystem>
                        <CustomSettings
                            type={extensionBeingEdited.packageType.value}
                            primaryButtonText="Next"
                            onChange={this.onChange.bind(this)}
                            actionButtonsDisabled={true}
                            isAddMode={true}
                            />
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                            <Button type="primary" onClick={this.onSave.bind(this)}>Update</Button>
                        </GridCell>
                    </GridCell>
                </SocialPanelBody>
            </div>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

NewExtensionModal.PropTypes = {
    onCancel: PropTypes.func
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        triedToSave: state.extension.triedToSave,
        installedPackageTypes: state.extension.installedPackageTypes,
        moduleCategories: state.extension.moduleCategories,
        locales: state.extension.locales,
        localePackages: state.extension.localePackages
    };
}

export default connect(mapStateToProps)(NewExtensionModal);