import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import InstallLog from "./InstallLog";
import { ExtensionActions, InstallationActions, PaginationActions } from "actions";
import PackageInformation from "../EditExtension/PackageInformation";
import ReleaseNotes from "../Editextension/ReleaseNotes";
import License from "../EditExtension/License";
import Button from "dnn-button";
import Localization from "localization";
import utilities from "utils";
import FileUpload from "./FileUpload";
import Checkbox from "dnn-checkbox";
import styles from "./style.less";
class InstallExtensionModal extends Component {
    constructor(props) {
        super();
        this.state = {
            package: null,
            wizardStep: 0,
            repairInstallChecked: false,
            selectedLegacyType: null,
            isPortalPackage: props.isPortalPackage
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
        props.dispatch(InstallationActions.navigateWizard(wizardStep));
    }

    parsePackage(file, callback, errorCallback) {
        if (!file) {
            utilities.utilities.notifyError(Localization.get("InstallExtension_EmptyPackage.Error"));
            return;
        }
        const { props } = this;
        this.setState({
            package: file
        }, () => {
            props.dispatch(InstallationActions.parsePackage(file, data => {
                data = JSON.parse(data);
                if (!data.success) {
                    if (errorCallback) {
                        errorCallback(data);
                    }
                }
                if (data.noManifest) {
                    this.setState({
                        selectedLegacyType: "Skin"
                    });
                }
                if (data.alreadyInstalled) {
                    this.setAlreadyInstalled(true);
                }
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            }));
        });
    }

    goToReleaseNotes() {
        this.goToStep(2);
    }

    goToLicense() {
        this.goToStep(3);
    }

    installPackage() {
        const { props } = this;
        this.setState({
            installingPackage: true
        });
        if (!props.installingAvailablePackage) {
            props.dispatch(InstallationActions.installExtension(this.state.package, props.parsedInstallationPackage, this.state.selectedLegacyType, this.state.isPortalPackage, () => {
                this.goToStep(4);
                this.setState({
                    installingPackage: false
                });
            }, !props.parsedInstallationPackage.alreadyInstalled));
        } else {
            props.dispatch(ExtensionActions.installAvailablePackage(props.availablePackage.PackageType, props.availablePackage.FileName, props.parsedInstallationPackage, () => {
                this.setState({
                    installingPackage: false
                });
                this.goToStep(4);
            }));
        }
    }

    onCheckRepairInstall(value) {
        this.setState({
            repairInstallChecked: value
        });
    }

    cancelInstall() {
        const { props } = this;
        props.dispatch(InstallationActions.clearParsedInstallationPackage());
        props.dispatch(InstallationActions.notInstallingAvailablePackage());
        props.dispatch(InstallationActions.toggleAcceptLicense(false));
        this.goToStep(0);
        props.onCancel();

        if (props.backToReferrerFunc) {
            props.backToReferrerFunc();
        }
    }

    getResxFromLegacyType() {
        if (this.state.selectedLegacyType === "Skin") {
            return Localization.get("CatalogSkin");
        } else {
            return Localization.get("Container");
        }
    }

    getPackageInformationStep() {
        const { props } = this;
        const parsedInstallationPackageCopy = utilities.utilities.getObjectCopy(props.parsedInstallationPackage);
        let parsedInstallationPackage = this.state.selectedLegacyType ?
            Object.assign(parsedInstallationPackageCopy, {
                packageType: this.getResxFromLegacyType(),
                friendlyName: this.state.package && this.state.package.name.replace(".zip", ""),
                name: this.state.package && this.state.package.name.replace(".zip", ""),
                version: "1.0.0"
            })
            : parsedInstallationPackageCopy;
        if (parsedInstallationPackage) {
            return <PackageInformation
                extensionBeingEdited={parsedInstallationPackage}
                validationMapped={false}
                onCancel={this.cancelInstall.bind(this)}
                installationMode={true}
                onSave={this.goToReleaseNotes.bind(this)}
                primaryButtonText={Localization.get("Next")}
                disabled={true} />;
        }
    }

    endInstallation() {
        const { props } = this;
        if (props.installingAvailablePackage) {
            props.dispatch(PaginationActions.loadTab(0, () => {
                const _packageType = (props.availablePackage && props.availablePackage.PackageType) ? props.availablePackage.PackageType : "Module";
                props.dispatch(ExtensionActions.getInstalledPackages(_packageType));
                props.dispatch(ExtensionActions.getAvailablePackages(_packageType));
            }));
        } else {
            if (props.tabIndex !== 0) {
                props.dispatch(PaginationActions.loadTab(0));
            }
            if (props.parsedInstallationPackage.packageType && props.parsedInstallationPackage.packageType !== props.selectedInstalledPackageType) {
                const _packageType = (props.parsedInstallationPackage && props.parsedInstallationPackage.packageType) ? props.parsedInstallationPackage.packageType : "Module";
                props.dispatch(ExtensionActions.getInstalledPackages(_packageType));
            } else if (this.state.selectedLegacyType) {
                props.dispatch(ExtensionActions.getInstalledPackages(this.state.selectedLegacyType));
            }
            else if (props.selectedInstalledPackageType) {
                props.dispatch(ExtensionActions.getInstalledPackages(props.selectedInstalledPackageType));
            }
        }

        this.cancelInstall();
    }
    onToggleLicenseAccept() {
        this.props.dispatch(InstallationActions.toggleAcceptLicense(!this.props.licenseAccepted));
    }
    toggleViewLog(value) {
        this.props.dispatch(InstallationActions.setViewingLog(value));
    }
    clearParsedInstallationPackage() {
        this.setAlreadyInstalled(false);
        this.props.dispatch(InstallationActions.clearParsedInstallationPackage());
    }
    cancelInstallationOnUpload() {
        this.clearParsedInstallationPackage();
        this.props.onCancel();
        this.props.dispatch(InstallationActions.toggleAcceptLicense(false));

        if (this.props.backToReferrerFunc) {
            this.props.backToReferrerFunc();
        }
    }
    onSelectLegacyType(value) {
        this.setState({
            selectedLegacyType: value
        });
    }
    setAlreadyInstalled(value, nextStepIfFalse) {
        this.setState({
            alreadyInstalled: value
        });
        if (nextStepIfFalse && !value) {
            this.goToStep(1);
        }
    }
    getSelectedLegacyTypeIsInstalled() {
        if (this.state.selectedLegacyType === "Skin") {
            return this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.legacySkinInstalled;
        }
        if (this.state.selectedLegacyType === "Container") {
            return this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.legacyContainerInstalled;
        }
    }
    render() {
        const { props } = this;
        const { wizardStep } = props,
            legacyInstalled = (props.parsedInstallationPackage && (props.parsedInstallationPackage.legacySkinInstalled || props.parsedInstallationPackage.legacyContainerInstalled)),
            legacyTypeIsInstalled = this.getSelectedLegacyTypeIsInstalled();
        return (
            <GridCell className={styles.installExtensionModal}>
                <PersonaBarPageHeader title={Localization.get("ExtensionInstall.Action")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: props.backToReferrer && props.backToReferrerText ? props.backToReferrerText : Localization.get("BackToExtensions"),
                    onClick: this.cancelInstall.bind(this)
                }}>
                    <GridCell className="install-extension-box extension-form">
                        {wizardStep === 0 &&
                            <GridCell>
                                <h3 className="box-title">{Localization.get("InstallExtension_UploadPackage.Header")}</h3>
                                <p>{Localization.get("InstallExtension_UploadPackage.HelpText")}</p>
                                <GridCell className="upload-package-box">
                                    <FileUpload
                                        parsePackage={this.parsePackage.bind(this)}
                                        repairInstall={this.goToStep.bind(this, 1)}
                                        cancelInstall={this.cancelInstall.bind(this)}
                                        parsedInstallationPackage={props.parsedInstallationPackage}
                                        alreadyInstalled={this.state.alreadyInstalled}
                                        toggleViewLog={this.toggleViewLog.bind(this)}
                                        clearParsedInstallationPackage={this.clearParsedInstallationPackage.bind(this)}
                                        viewingLog={props.viewingLog}
                                        onSelectLegacyType={this.onSelectLegacyType.bind(this)}
                                        selectedLegacyType={this.state.selectedLegacyType}
                                    />
                                </GridCell>
                                <GridCell className="modal-footer">
                                    <Button onClick={!props.viewingLog ? this.cancelInstallationOnUpload.bind(this) : this.toggleViewLog.bind(this, false)}>{Localization.get("InstallExtension_Cancel.Button")}</Button>
                                    <Button onClick={legacyInstalled ? this.setAlreadyInstalled.bind(this, legacyTypeIsInstalled, true) : this.goToStep.bind(this, 1)} type="primary"
                                        disabled={(!props.parsedInstallationPackage || !props.parsedInstallationPackage.success)}>
                                        {Localization.get("InstallExtension_Upload.Button")}
                                    </Button>
                                </GridCell>
                            </GridCell>
                        }
                        {wizardStep === 1 && this.getPackageInformationStep()}
                        {wizardStep === 2 &&
                            <ReleaseNotes
                                value={props.parsedInstallationPackage.releaseNotes}
                                onCancel={this.cancelInstall.bind(this)}
                                onSave={this.goToLicense.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")}
                                installationMode={true}
                                readOnly={true}
                                disabled={true} />}
                        {wizardStep === 3 &&
                            <License
                                value={props.parsedInstallationPackage.license}
                                onCancel={this.cancelInstall.bind(this)}
                                installationMode={true}
                                readOnly={true}
                                onSave={this.installPackage.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")}
                                disabled={true}
                                primaryButtonDisabled={!props.licenseAccepted || this.state.installingPackage}
                                acceptLicenseCheckbox={
                                    <Checkbox
                                        label={Localization.get("InstallExtension_AcceptLicense.Label")}
                                        value={props.licenseAccepted}
                                        onCancel={this.cancelInstall.bind(this)}
                                        onChange={this.onToggleLicenseAccept.bind(this)} />}
                            />}
                        {wizardStep === 4 &&
                            <InstallLog
                                logs={props.installationLogs}
                                onDone={this.endInstallation.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")} />}

                        <p className="modal-pagination">{"-- " + (props.wizardStep + 1) + " of 5 --"} </p>
                    </GridCell>
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

InstallExtensionModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    parsedInstallationPackage: PropTypes.object,
    selectedInstalledPackageType: PropTypes.string,
    wizardStep: PropTypes.number,
    installationLogs: PropTypes.array,
    installingAvailablePackage: PropTypes.bool,
    availablePackage: PropTypes.object,
    licenseAccepted: PropTypes.bool,
    viewingLog: PropTypes.bool,
    isPortalPackage: PropTypes.bool,
    backToReferrer: PropTypes.string,
    backToReferrerText: PropTypes.string,
    backToReferrerFunc: PropTypes.func
};

function mapStateToProps(state) {
    return {
        parsedInstallationPackage: state.installation.parsedInstallationPackage,
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        wizardStep: state.installation.installWizardStep,
        installationLogs: state.installation.installationLogs,
        installingAvailablePackage: state.installation.installingAvailablePackage,
        availablePackage: state.installation.availablePackage,
        licenseAccepted: state.installation.licenseAccepted,
        viewingLog: state.installation.viewingLog,
        isPortalPackage: state.installation.isPortalPackage
    };
}

export default connect(mapStateToProps)(InstallExtensionModal);