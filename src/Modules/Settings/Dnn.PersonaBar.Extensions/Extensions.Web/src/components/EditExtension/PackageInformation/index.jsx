import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Button from "dnn-button";
import Localization from "localization";
import {
    formatVersionNumber,
    validationMapExtensionBeingEdited,
    valueMapExtensionBeingEdited,
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

    resetExtensionBeingEdited() {
        const { props } = this;
        this.setState({
            extensionBeingEdited: validationMapExtensionBeingEdited(props.extensionBeingEdited)
        });
    }

    componentWillMount() {
        this.resetExtensionBeingEdited();
    }

    onChange(key, event) {
        const { props, state } = this;
        const value = typeof event === "object" ? event.target.value : event;
        let {triedToSave, extensionBeingEdited} = state;

        extensionBeingEdited[key].value = value;
        if (extensionBeingEdited[key].validateRequired && extensionBeingEdited[key].error) {
            extensionBeingEdited[key].error = false;
            triedToSave = false;
        }
        this.setState({
            triedToSave,
            extensionBeingEdited
        });
    }

    onCancel(event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        this.resetExtensionBeingEdited();
        props.onCancel();
    }

    validateFields() {
        let {triedToSave, extensionBeingEdited} = this.state;
        let errorCount = 0;
        Object.keys(extensionBeingEdited).forEach((key) => {
            let field = extensionBeingEdited[key];
            if (field.validateRequired && field.value === "") {
                field.error = true;
                errorCount++;
            }
        });
        triedToSave = true;
        this.setState({
            extensionBeingEdited,
            triedToSave
        });
        return errorCount === 0;
    }

    onSave() {
        if (!this.validateFields()) {
            return;
        }
        const { props } = this;
        props.onPrimaryButtonClick(valueMapExtensionBeingEdited(this.state.extensionBeingEdited));
    }

    onVersionChange(index, option) {
        let {extensionBeingEdited} = this.state;
        if (extensionBeingEdited.version && extensionBeingEdited.version.value) {
            let versionArray = extensionBeingEdited.version.value.split(".");
            versionArray[index] = option.value;
            extensionBeingEdited.version.value = versionArray.join(".");
        }
        this.setState({
            extensionBeingEdited
        });
    }

    render() {
        const {props, state} = this;

        if (!props.extensionBeingEdited) {
            return <p>Empty</p>;
        }
        const {extensionBeingEdited} = state;
        const version = extensionBeingEdited.version.value ? extensionBeingEdited.version.value.split(".") : "0.0.0";

        return (
            <GridCell className={styles.packageInformationBox}>
                <GridCell columnSize={100} style={{ marginBottom: 15 }}>
                    <DropdownWithError
                        options={[{ label: "Container", value: "blah" }]}
                        tooltipMessage={Localization.get("EditExtension_PackageType.HelpText")}
                        label="Extension Type"
                        defaultDropdownValue={extensionBeingEdited.packageType.value}
                        style={inputStyle}
                        enabled={false}
                        />
                </GridCell>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageName.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                            style={inputStyle}
                            enabled={false}
                            value={extensionBeingEdited.name.value} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageFriendlyName.Label") + "*"}
                            tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                            value={extensionBeingEdited.friendlyName.value}
                            style={inputStyle}
                            error={extensionBeingEdited.friendlyName.error && state.triedToSave}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "friendlyName")} />

                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageIconFile.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageIconFile.HelpText")}
                            value={extensionBeingEdited.packageIcon.value}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "packageIcon")} />
                    </div>
                    <div>
                        <MultiLineInputWithError
                            label={Localization.get("EditExtension_PackageDescription.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageDescription.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 28, height: 123 }}
                            value={extensionBeingEdited.description.value}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "description")} />
                        <DropdownWithError
                            options={getVersionDropdownValues()}
                            label={Localization.get("EditExtension_PackageVersion.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageVersion.HelpText")}
                            enabled={!props.disabled}
                            defaultDropdownValue={formatVersionNumber(version[0])}
                            onSelect={this.onVersionChange.bind(this, 0)}
                            className="version-dropdown"
                            />
                        <Dropdown
                            options={getVersionDropdownValues()}
                            className="version-dropdown"
                            label={formatVersionNumber(version[1])}
                            onSelect={this.onVersionChange.bind(this, 1)}
                            enabled={!props.disabled}
                            />
                        <Dropdown
                            options={getVersionDropdownValues()}
                            label={formatVersionNumber(version[2])}
                            className="version-dropdown"
                            onSelect={this.onVersionChange.bind(this, 2)}
                            enabled={!props.disabled}
                            />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>
                <GridSystem className="with-right-border bottom-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageOwner.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageOwner.HelpText")}
                            style={inputStyle}
                            value={extensionBeingEdited.owner.value}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "owner")} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageOrganization.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageOrganization.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 0 }}
                            value={extensionBeingEdited.organization.value}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "organization")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageURL.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageURL.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={extensionBeingEdited.url.value}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "url")} />
                        <SingleLineInputWithError
                            label={Localization.get("EditExtension_PackageEmailAddress.Label")}
                            tooltipMessage={Localization.get("EditExtension_PackageEmailAddress.HelpText")}
                            style={inputStyle}
                            inputStyle={{ marginBottom: 32 }}
                            value={extensionBeingEdited.email.value}
                            enabled={!props.disabled}
                            onChange={this.onChange.bind(this, "email")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={this.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary" onClick={this.onSave.bind(this)}>{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

PackageInformation.PropTypes = {
    onCancel: PropTypes.func,
    onPrimaryButtonClick: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default PackageInformation;