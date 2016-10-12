import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import InstallLog from "./InstallLog";
import { extension as ExtensionActions } from "actions";
import PackageInformation from "../EditExtension/PackageInformation";
import ReleaseNotes from "../Editextension/ReleaseNotes";
import License from "../EditExtension/License";
import Button from "dnn-button";
import Checkbox from "dnn-checkbox";
import styles from "./style.less";

class InstallExtensionModal extends Component {
    constructor() {
        super();
        this.state = {
            package: null,
            wizardStep: 0,
            repairInstallChecked: false
        };
    }

    onPackageChange(event) {
        const files = event.target.files;

        if (files && files.length > 0) {
            this.setState({
                package: files[0]
            });
        }
    }

    goToStep(wizardStep) {
        const { props } = this;
        props.dispatch(ExtensionActions.navigateWizard(wizardStep));
    }

    parsePackage() {
        const {props} = this;
        props.dispatch(ExtensionActions.parsePackage(this.state.package, () => {
            this.goToStep(1);
        }));
    }

    goToReleaseNotes() {
        this.goToStep(2);
    }

    goToLicense() {
        this.goToStep(3);
    }

    installPackage() {
        const {props} = this;
        props.dispatch(ExtensionActions.installExtension(this.state.package, () => {
            this.goToStep(4);
        }));
    }

    onCheckRepairInstall(value) {
        console.log(value);
        this.setState({
            repairInstallChecked: value
        });
    }

    cancelInstall() {
        const {props} = this;
        props.dispatch(ExtensionActions.clearParsedInstallationPackage(() => {
            this.goToStep(0);
        }));
        this.setState({
            package: null
        });
    }

    getPackageInformationStep() {
        const {props, state} = this;
        if (props.parsedInstallationPackage && !props.parsedInstallationPackage.alreadyInstalled) {
            return <PackageInformation
                extensionBeingEdited={props.parsedInstallationPackage}
                onChange={() => { } }
                onCancel={this.endInstallation.bind(this)}
                onUpdateExtension={this.goToReleaseNotes.bind(this)}
                primaryButtonText="Next"
                disabled={true} />;
        } else {
            return <GridCell>
                This package is already installed
                <GridCell>
                    <Checkbox label="Repair install?" value={state.repairInstallChecked} onChange={this.onCheckRepairInstall.bind(this)} />
                </GridCell>
                <GridCell className="modal-footer">
                    <Button onClick={this.cancelInstall.bind(this)}>Cancel</Button>
                    <Button onClick={this.goToReleaseNotes.bind(this)} type="primary" disabled={!state.repairInstallChecked}>Next</Button>
                </GridCell>
            </GridCell>;
        }
    }

    endInstallation() {
        const { props } = this;
        props.onCancel();
        this.cancelInstall();
    }

    render() {
        const {props, state} = this;
        const {wizardStep} = props;
        return (
            <GridCell className={styles.installExtensionModal}>
                <SocialPanelHeader title="Install Extension" />
                <SocialPanelBody>
                    <GridCell className="new-extension-box">
                        {wizardStep === 0 &&
                            <GridCell>
                                <h3 className="box-title">Upload Extension Package</h3>
                                <p>To begin installation, upload the package by dragging the file into the field below.</p>
                                <GridCell className="upload-package-box">
                                    <input type="file" onChange={this.onPackageChange.bind(this)} />
                                </GridCell>
                                <GridCell className="modal-footer">
                                    <Button onClick={props.onCancel.bind(this)}>Cancel</Button>
                                    <Button onClick={this.parsePackage.bind(this)} type="primary">Upload</Button>
                                </GridCell>
                            </GridCell>
                        }
                        {wizardStep === 1 && this.getPackageInformationStep()}
                        {wizardStep === 2 &&
                            <ReleaseNotes
                                extensionBeingEdited={props.parsedInstallationPackage}
                                onChange={() => { } }
                                onCancel={this.endInstallation.bind(this)}
                                onUpdateExtension={this.goToLicense.bind(this)}
                                primaryButtonText="Next"
                                disabled={true} />}
                        {wizardStep === 3 &&
                            <License
                                extensionBeingEdited={props.parsedInstallationPackage}
                                onChange={() => { } }
                                onCancel={this.endInstallation.bind(this)}
                                readOnly={true}
                                onUpdateExtension={this.installPackage.bind(this)}
                                primaryButtonText="Next"
                                disabled={true} />}
                        {wizardStep === 4 &&
                            <InstallLog
                                logs={props.installationLogs}
                                onDone={this.endInstallation.bind(this)}
                                primaryButtonText="Next" />}

                        <p className="modal-pagination">{"-- " + (props.wizardStep + 1) + " of 5 --"} </p>
                    </GridCell>
                </SocialPanelBody>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

InstallExtensionModal.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func
};

function mapStateToProps(state) {
    return {
        parsedInstallationPackage: state.extension.parsedInstallationPackage,
        wizardStep: state.extension.installWizardStep,
        installationLogs: state.extension.installationLogs
    };
}

export default connect(mapStateToProps)(InstallExtensionModal);