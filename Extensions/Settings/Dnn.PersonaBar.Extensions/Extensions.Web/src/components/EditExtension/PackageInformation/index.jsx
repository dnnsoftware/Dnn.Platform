import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SingleLineInputWithError, MultiLineInputWithError, DropdownWithError, GridSystem, Dropdown, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import {
    formatVersionNumber,
    validationMapExtensionBeingEdited,
    getVersionDropdownValues
} from "./helperFunctions";
import styles from "./style.less";

const inputStyle = { width: "100%" };


class PackageInformation extends Component {
    constructor() {
        super();
        this.state = {
            extensionBeingEdited: validationMapExtensionBeingEdited({
                packageType: "",
                name: "",
                description: "",
                friendlyName: "",
                version: "9.0.0",
                owner: "",
                url: "",
                organization: "",
                email: ""
            }),
            triedToSave: false
        };
    }

    render() {
        const {props} = this;

        if (!props.extensionBeingEdited) {
            return <p>Empty</p>;
        }
        const {extensionBeingEdited} = props;
        const version = props.validationMapped ? (extensionBeingEdited.version.value ? extensionBeingEdited.version.value.split(".") : [0, 0, 0]) : (extensionBeingEdited.version ? extensionBeingEdited.version.split(".") : [0, 0, 0]);

        return (
            <GridCell className={styles.pkgInformationBox}>
                {props.installationMode &&
                    <GridCell className="package-information-header">
                        <h6>{Localization.get("InstallExtension_PackageInfo.Header")}</h6>
                        <p>{Localization.get("InstallExtension_PackageInfo.HelpText")}</p>
                        <hr />
                    </GridCell>
                }
                <GridCell columnSize={100} style={{ marginBottom: 15 }}>
                    <DropdownWithError
                        tooltipMessage={Localization.get("EditExtension_PackageType.HelpText")}
                        label={Localization.get("EditExtension_PackageType.Label")}
                        defaultDropdownValue={!props.validationMapped ? extensionBeingEdited.packageType : extensionBeingEdited.packageType.value}
                        style={inputStyle}
                        enabled={false} />
                </GridCell>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageName.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                            style={inputStyle}
                            enabled={false}
                            value={!props.validationMapped ? extensionBeingEdited.name : extensionBeingEdited.name.value} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageFriendlyName.Label") + "*"}
                            tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                            value={!props.validationMapped ? extensionBeingEdited.friendlyName : extensionBeingEdited.friendlyName.value}
                            style={inputStyle}
                            error={extensionBeingEdited.friendlyName.error && props.triedToSave}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "friendlyName")} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageIconFile.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageIconFile.HelpText")}
                            value={!props.validationMapped ? extensionBeingEdited.packageIcon : extensionBeingEdited.packageIcon.value}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "packageIcon")} />
                    </div>
                    <div>
                        <MultiLineInputWithError
                            label={Localization.get("EditExtension_PackageDescription.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageDescription.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 28, height: 123 }}
                            value={!props.validationMapped ? extensionBeingEdited.description : extensionBeingEdited.description.value}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "description")} />
                        <DropdownWithError
                            options={getVersionDropdownValues()}
                            label={Localization.get("EditExtension_PackageVersion.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageVersion.HelpText")}
                            enabled={!props.disabled}
                            defaultDropdownValue={formatVersionNumber(version[0])}
                            onSelect={props.onVersionChange && props.onVersionChange.bind(this, 0)}
                            className="version-dropdown"
                            style={{position: "relative", top: 3}} />
                        <Dropdown
                            options={getVersionDropdownValues()}
                            className="version-dropdown"
                            label={formatVersionNumber(version[1])}
                            onSelect={props.onVersionChange && props.onVersionChange.bind(this, 1)}
                            enabled={!props.disabled} />
                        <Dropdown
                            options={getVersionDropdownValues()}
                            label={formatVersionNumber(version[2])}
                            className="version-dropdown"
                            onSelect={props.onVersionChange && props.onVersionChange.bind(this, 2)}
                            enabled={!props.disabled} />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>
                <GridSystem className="with-right-border bottom-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageOwner.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageOwner.HelpText")}
                            style={inputStyle}
                            value={!props.validationMapped ? extensionBeingEdited.owner : extensionBeingEdited.owner.value}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "owner")} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageOrganization.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageOrganization.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            value={!props.validationMapped ? extensionBeingEdited.organization : extensionBeingEdited.organization.value}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "organization")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageURL.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageURL.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={!props.validationMapped ? extensionBeingEdited.url : extensionBeingEdited.url.value}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "url")} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageEmailAddress.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageEmailAddress.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={!props.validationMapped ? extensionBeingEdited.email : extensionBeingEdited.email.value}
                            enabled={!props.disabled}
                            onChange={props.onChange && props.onChange.bind(this, "email")} />
                    </div>
                </GridSystem>
                {!props.buttonsAreHidden && <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                    {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                    {(!props.disabled || props.installationMode) && <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>}
                </GridCell>}
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

PackageInformation.propTypes = {
    onCancel: PropTypes.func,
    onPrimaryButtonClick: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.bool,
    primaryButtonText: PropTypes.string,
    triedToSave: PropTypes.bool,
    validateFields: PropTypes.func,
    validationMapped: PropTypes.bool
};

export default PackageInformation;
