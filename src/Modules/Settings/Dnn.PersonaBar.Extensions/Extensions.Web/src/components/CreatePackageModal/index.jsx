import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { CreatePackageActions } from "actions";
import { ActionCreators as UndoActionCreators } from "redux-undo";
import StepOne from "./StepOne";
import StepTwo from "./StepTwo";
import StepThree from "./StepThree";
import StepFour from "./StepFour";
import StepFive from "./StepFive";
import Localization from "localization";
import styles from "./style.less";

class CreatePackage extends Component {
    constructor() {
        super();

        this.state = {
            useExistingManifest: false,
            selectedManifest: "",
            selectedManifestKey: "",
            archiveName: "",
            manifestName: "",
            createManifest: true,
            createPackage: true,
            reviewManifest: true
        };
    }

    componentWillMount() {
        this.goToStep(0);
        let { props } = this;
        let _packagePayload = JSON.parse(JSON.stringify(props.packagePayload));

        _packagePayload.archiveName = props.packageManifest.name + "_" + props.packageManifest.version + "_Install.zip";

        _packagePayload.manifestName = props.packageManifest.name + ".dnn";
        _packagePayload.selectedManifest = props.packageManifest.manifests[Object.keys(props.packageManifest.manifests)[0]];

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
    }

    goToStep(step) {
        const { props } = this;
        props.dispatch(CreatePackageActions.goToWizardStep(step));
    }

    onChange(key, event) {
        let value = typeof event === "object" ? event.target.value : event;
        const { props } = this;
        let _packagePayload = JSON.parse(JSON.stringify(props.packagePayload));

        _packagePayload[key] = value;

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
    }

    onSelect(key, option) {
        let value = option.value;

        const { props } = this;
        let _packagePayload = JSON.parse(JSON.stringify(props.packagePayload));

        _packagePayload[key] = value;
        _packagePayload.selectedManifestKey = option.label;

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
    }

    getManifestDropdown(manifests) {
        return Object.keys(manifests).map((key) => {
            return {
                label: key,
                value: manifests[key]
            };
        });
    }

    goToStepTwo() {
        const { packagePayload } = this.props;
        if (packagePayload.useExistingManifest) {
            this.goToStepFour();
        } else {
            this.goToStep(1);
        }
    }

    goToStepThree() {
        const { props } = this;
        if (props.packagePayload.useExistingManifest) {
            this.goToStep(3);
        } else {
            this.goToStep(1);
        }
    }

    goToStepFour() {
        const { props } = this;
        if (props.packagePayload.reviewManifest) {
            this.goToStep(3);
        } else {
            this.goToStep(4);
        }
    }

    createPackage() {
    }

    onStepBack() {
        const { props } = this;
        props.dispatch(UndoActionCreators.undo());
    }

    onFileOrAssemblyChange(key, event) {
        const { props } = this;
        let value = typeof event === "object" ? event.target.value : event;

        const _fileValue = value.split("\n");

        let packageManifest = JSON.parse(JSON.stringify(props.packageManifest));

        packageManifest[key] = _fileValue;

        props.dispatch(CreatePackageActions.updatePackageManifest(packageManifest));
    }

    onBasePathChange(event) {
        let value = typeof event === "object" ? event.target.value : event;

        const { props } = this;

        let packageManifest = JSON.parse(JSON.stringify(props.packageManifest));

        packageManifest.basePath = value;

        props.dispatch(CreatePackageActions.updatePackageManifest(packageManifest));
    }

    getCurrentWizardUI(step) {
        const { props } = this;
        const {packageManifest, installedPackageTypes, packagePayload} = props;
        const version = packageManifest.version ? packageManifest.version.split(".") : [0, 0, 0];
        const manifestDropdown = this.getManifestDropdown(props.packageManifest.manifests);
        switch (step) {
            case 0:
                return <StepOne
                    packageManifest={packageManifest}
                    version={version}
                    installedPackageTypes={installedPackageTypes}
                    useExistingManifest={packagePayload.useExistingManifest}
                    onNext={this.goToStepTwo.bind(this)}
                    onChange={this.onChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    manifestDropdown={manifestDropdown}
                    selectedManifest={packagePayload.selectedManifest}
                    onSelect={this.onSelect.bind(this)}
                    reviewManifest={packagePayload.reviewManifest}
                    hasManifests={Object.keys(packageManifest.manifests).length > 0}
                    />;
            case 1:
                return <StepTwo
                    packageManifest={packageManifest}
                    onNext={this.goToStep.bind(this, 2)}
                    onBasePathChange={this.onBasePathChange.bind(this)}
                    onFileOrAssemblyChange={this.onFileOrAssemblyChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onPrevious={this.onStepBack.bind(this)}
                    />;
            case 2:
                return <StepThree
                    packageManifest={packageManifest}
                    onNext={this.goToStepFour.bind(this)}
                    onBasePathChange={this.onBasePathChange.bind(this)}
                    onFileOrAssemblyChange={this.onFileOrAssemblyChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onPrevious={this.onStepBack.bind(this)}
                    />;
            case 3:
                return <StepFour
                    packageManifest={packageManifest}
                    selectedManifest={packagePayload.selectedManifest}
                    onNext={this.goToStep.bind(this, 4)}
                    onChange={this.onChange.bind(this)}
                    onPrevious={this.onStepBack.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    />;
            case 4:
                return <StepFive
                    onNext={this.createPackage.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onChange={this.onChange.bind(this)}
                    createPackage={packagePayload.createPackage}
                    createManifest={packagePayload.createManifest}
                    manifestName={packagePayload.manifestName}
                    archiveName={packagePayload.archiveName}
                    useExistingManifest={packagePayload.useExistingManifest}
                    onPrevious={this.onStepBack.bind(this)}
                    />;
            default:
                return <p>Oops, something went wrong. Click <a href="javascript:void(0)" onClick={props.onCancel.bind(this)}> here </a> to go back</p>;
        }
    }

    render() {
        const {props, state} = this;
        const {createPackageStep} = props;
        return (
            <GridCell className={styles.createPackage}>
                <SocialPanelHeader title={Localization.get("CreatePackage_Header.Header")} />
                <SocialPanelBody>
                    <GridCell className="extension-form create-package-wizard">
                        {this.getCurrentWizardUI(createPackageStep)}
                    </GridCell>
                </SocialPanelBody>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

CreatePackage.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    packageManifest: PropTypes.object,
    installedPackageTypes: PropTypes.array,
    createPackageStep: PropTypes.number,
    packagePayload: PropTypes.object
};

function mapStateToProps(state) {
    return {
        installedPackageTypes: state.extension.installedPackageTypes,
        packageManifest: state.createPackage.present.packageManifest,
        packagePayload: state.createPackage.present.packagePayload,
        createPackageStep: state.createPackage.present.currentStep
    };
}

export default connect(mapStateToProps)(CreatePackage);