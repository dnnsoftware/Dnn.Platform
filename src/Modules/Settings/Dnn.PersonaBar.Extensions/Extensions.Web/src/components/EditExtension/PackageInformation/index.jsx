import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}
class PackageInformation extends Component {
    render() {
        const {props, state} = this;
        
        if (!props.extensionBeingEdited) {
            return <p>Empty</p>;
        }
        const {extensionBeingEdited} = props;
        const version = extensionBeingEdited.version ? extensionBeingEdited.version.split(".") : "00.00.00";
        return (
            <GridCell className={styles.newExtensionBox}>
                <GridCell columnSize={100} style={{ marginBottom: 15 }}>
                    <DropdownWithError
                        options={[{ label: "Container", value: "blah" }]}
                        tooltipMessage={Localization.get("EditExtension_PackageType.HelpText")}
                        label="Extension Type"
                        defaultDropdownValue={extensionBeingEdited.packageType}
                        style={inputStyle}
                        enabled={false}
                        />
                </GridCell>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label="Name"
                            tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                            style={inputStyle}
                            enabled={false}
                            value={extensionBeingEdited.name} />
                        <SingleLineInputWithError
                            label="Friendly Name"
                            tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                            value={extensionBeingEdited.friendlyName}
                            style={inputStyle}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "friendlyName")} />

                        <SingleLineInputWithError
                            label="Icon"
                            tooltipMessage={Localization.get("EditExtension_PackageIconFile.HelpText")}
                            value={extensionBeingEdited.packageIcon}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "packageIcon")} />
                    </div>
                    <div>
                        <MultiLineInputWithError
                            label="Description"
                            tooltipMessage={Localization.get("EditExtension_PackageDescription.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 28, height: 123 }}
                            value={extensionBeingEdited.description}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "description")} />
                        <DropdownWithError
                            options={[{ label: "Container", value: "blah" }]}
                            tooltipMessage={Localization.get("EditExtension_PackageVersion.HelpText")}
                            label="Version"
                            enabled={!props.disabled}
                            defaultDropdownValue={formatVersionNumber(version[0])}
                            className="version-dropdown"
                            />
                        <Dropdown
                            options={[{ label: "Container", value: "blah" }]}
                            className="version-dropdown"
                            label={formatVersionNumber(version[1])}
                            enabled={!props.disabled}
                            />
                        <Dropdown
                            options={[{ label: "Container", value: "blah" }]}
                            label={formatVersionNumber(version[2])}
                            className="version-dropdown"
                            enabled={!props.disabled}
                            />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>
                <GridSystem className="with-right-border bottom-half">
                    <div>
                        <SingleLineInputWithError
                            label="Owner"
                            tooltipMessage={Localization.get("EditExtension_PackageOwner.HelpText")}
                            style={inputStyle}
                            value={extensionBeingEdited.owner}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "owner")} />
                        <SingleLineInputWithError
                            label="Organization"
                            tooltipMessage={Localization.get("EditExtension_PackageOrganization.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            value={extensionBeingEdited.organization}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "organization")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label="URL"
                            tooltipMessage={Localization.get("EditExtension_PackageURL.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={extensionBeingEdited.url}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "url")} />
                        <SingleLineInputWithError
                            label="Email Address"
                            tooltipMessage={Localization.get("EditExtension_PackageEmailAddress.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={extensionBeingEdited.email}
                            enabled={!props.disabled}
                            onChange={props.onChange.bind(this, "email")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary" onClick={props.onUpdateExtension.bind(this)}>{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

PackageInformation.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default PackageInformation;