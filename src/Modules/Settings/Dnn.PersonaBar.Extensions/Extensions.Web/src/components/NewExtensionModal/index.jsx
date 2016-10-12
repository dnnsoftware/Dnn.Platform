import React, {PropTypes, Component} from "react";
import DropdownWithError from "dnn-dropdown-with-error";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import Localization from "localization";
import Dropdown from "dnn-dropdown";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Button from "dnn-button";
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
        const value = typeof event === "object" ? event.target.value : event;
        let {extensionBeingAdded} = this.state;
        extensionBeingAdded[key] = value;
        this.setState({
            extensionBeingAdded
        });
    }


    render() {
        const {props} = this;
        const {extensionBeingAdded} = this.state;
        const version = extensionBeingAdded.version.split(".");
        return (
            <div className={styles.newExtensionModal}>
                <SocialPanelHeader title="Create New Extension"/>
                <SocialPanelBody>
                    <GridCell className="new-extension-box">
                        <GridSystem className="with-right-border top-half">
                            <div>
                                <DropdownWithError
                                    className="extension-type"
                                    options={[{ label: "Container", value: "blah" }]}
                                    tooltipMessage={Localization.get("EditExtension_PackageType.HelpText") }
                                    label="Extension Type"
                                    defaultDropdownValue={extensionBeingAdded.type}
                                    style={inputStyle}
                                    />
                                <SingleLineInputWithError
                                    label="Name"
                                    tooltipMessage={Localization.get("EditExtension_PackageName.HelpText") }
                                    style={inputStyle}
                                    value={extensionBeingAdded.name}/>
                                <SingleLineInputWithError
                                    label="Friendly Name"
                                    tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText") }
                                    value={extensionBeingAdded.friendlyName}
                                    style={inputStyle}
                                    onChange={this.onChange.bind(this, "friendlyName") }/>
                            </div>
                            <div>
                                <MultiLineInputWithError
                                    label="Description"
                                    tooltipMessage={Localization.get("EditExtension_PackageDescription.HelpText") }
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 28, height: 123 }}
                                    value={extensionBeingAdded.description}
                                    onChange={this.onChange.bind(this, "description") }/>
                                <DropdownWithError
                                    options={this.versionDropdownOptions}
                                    tooltipMessage={Localization.get("EditExtension_PackageVersion.HelpText") }
                                    label="Version"
                                    defaultDropdownValue={formatVersionNumber(version[0]) }
                                    className="version-dropdown"
                                    />
                                <Dropdown
                                    options={this.versionDropdownOptions}
                                    className="version-dropdown"
                                    label={formatVersionNumber(version[1]) }
                                    />
                                <Dropdown
                                    options={this.versionDropdownOptions}
                                    label={formatVersionNumber(version[2]) }
                                    className="version-dropdown"
                                    />
                            </div>
                        </GridSystem>
                        <GridCell><hr/></GridCell>
                        <GridCell className="box-title-container">
                            <h3 className="box-title">{Localization.get("EditExtension_OwnerDetails.Label") }</h3>
                        </GridCell>
                        <GridSystem className="with-right-border bottom-half">
                            <div>
                                <SingleLineInputWithError
                                    label="Owner"
                                    tooltipMessage={Localization.get("EditExtension_PackageOwner.HelpText") }
                                    style={inputStyle}
                                    value={extensionBeingAdded.owner}
                                    onChange={this.onChange.bind(this, "owner") }/>
                                <SingleLineInputWithError
                                    label="Organization"
                                    tooltipMessage={Localization.get("EditExtension_PackageOrganization.HelpText") }
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 0 }}
                                    value={extensionBeingAdded.organization}
                                    onChange={this.onChange.bind(this, "organization") }/>
                            </div>
                            <div>
                                <SingleLineInputWithError
                                    label="URL"
                                    tooltipMessage={Localization.get("EditExtension_PackageURL.HelpText") }
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 32 }}
                                    value={extensionBeingAdded.url}
                                    onChange={this.onChange.bind(this, "url") }/>
                                <SingleLineInputWithError
                                    label="Email Address"
                                    tooltipMessage={Localization.get("EditExtension_PackageEmailAddress.HelpText") }
                                    style={inputStyle}
                                    inputStyle={{ marginBottom: 32 }}
                                    value={extensionBeingAdded.email}
                                    onChange={this.onChange.bind(this, "email") }/>
                            </div>
                        </GridSystem>
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

export default NewExtensionModal;