import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Button from "dnn-button";
import Tabs from "dnn-tabs";
import Localization from "localization";
import License from "./License";
import ReleaseNotes from "./ReleaseNotes";
import PackageInformation from "./PackageInformation";
import CustomSettings from "./CustomSettings";
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
            }
        };
    }

    componentWillMount() {
        const {props} = this;
        this.setState({
            extensionBeingEdited: props.extensionBeingEdited
        });

    }

    onVersionChange(index, event) {

    }

    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let {extensionBeingEdited} = this.state;
        extensionBeingEdited[key] = value;
        this.setState({
            extensionBeingEdited
        });
    }

    render() {
        const {props, state} = this;
        const {extensionBeingEdited} = state;
        return (
            <GridCell className={styles.editExtension}>
                <SocialPanelHeader title={extensionBeingEdited.friendlyName + " Extension"} />
                <SocialPanelBody>
                    <Tabs
                        tabHeaders={["Package Information", "Extension Settings", "Site Settings", "License", "Release Notes"]}
                        type="primary">
                        <GridCell className="package-information-box extension-form">
                            <PackageInformation
                                extensionBeingEdited={extensionBeingEdited}
                                onPrimaryButtonClick={props.onUpdateExtension.bind(this)}
                                onCancel={props.onCancel.bind(this)}
                                onChange={this.onChange.bind(this)}
                                primaryButtonText="Update" />
                        </GridCell>
                        <GridCell className="extension-form">
                            <CustomSettings type="Module" primaryButtonText="Next" />
                        </GridCell>
                        <GridCell>
                            Site Settings
                        </GridCell>
                        <License extensionBeingEdited={extensionBeingEdited}
                            onChange={this.onChange.bind(this)}
                            onCancel={props.onCancel.bind(this)}
                            onUpdateExtension={props.onUpdateExtension.bind(this, state.extensionBeingEdited)}
                            primaryButtonText="Update" />
                        <ReleaseNotes extensionBeingEdited={extensionBeingEdited}
                            onChange={this.onChange.bind(this)}
                            onCancel={props.onCancel.bind(this)}
                            onUpdateExtension={props.onUpdateExtension.bind(this, state.extensionBeingEdited)}
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
    disabled: PropTypes.func
};

export default EditExtension;