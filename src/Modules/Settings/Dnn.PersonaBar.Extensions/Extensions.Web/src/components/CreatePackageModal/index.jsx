import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { ExtensionActions, VisiblePanelActions, ModuleDefinitionActions, CreatePackageActions } from "actions";
import StepOne from "./StepOne";
import Localization from "localization";
import styles from "./style.less";

class CreatePackage extends Component {

    componentWillMount() {
        this.goToStep(0);
    }

    goToStep(step) {
        const { props } = this;
        props.dispatch(CreatePackageActions.goToWizardStep(step));
    }

    getCurrentWizardUI(step) {
        const { props } = this;
        const {extensionBeingEdited, installedPackageTypes} = props;
        const version = extensionBeingEdited.version.value ? extensionBeingEdited.version.value.split(".") : [0, 0, 0];
        switch (step) {
            case 0:
                return <StepOne
                    extensionBeingEdited={extensionBeingEdited}
                    version={version}
                    installedPackageTypes={installedPackageTypes}
                    onNext={this.goToStep.bind(this, 1)}
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
    extensionBeingEdited: PropTypes.object,
    installedPackageTypes: PropTypes.array,
    createPackageStep: PropTypes.number
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        installedPackageTypes: state.extension.installedPackageTypes,
        createPackageStep: state.createPackage.currentStep
    };
}

export default connect(mapStateToProps)(CreatePackage);