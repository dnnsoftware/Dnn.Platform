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
import { ExtensionActions } from "actions";
import Button from "dnn-button";
import CustomSettings from "../EditExtension/CustomSettings";
import BasicPackageInformation from "../common/BasicPackageInformation";
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
        this.state = {
            extensionBeingAdded: {
                description: "",
                email: "",
                friendlyName: "",
                name: "",
                organization: "",
                owner: "",
                url: "",
                version: "0.0.0"
            }
        };
        this.versionDropdownOptions = getDropdownOptions();
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
                            />
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                            <Button type="primary">Update</Button>
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
        installedPackageTypes: state.extension.installedPackageTypes
    };
}

export default connect(mapStateToProps)(NewExtensionModal);