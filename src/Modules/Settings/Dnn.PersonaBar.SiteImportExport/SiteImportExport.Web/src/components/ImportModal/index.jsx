import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
} from "../../actions";
import {
    TemplateIcon
} from "dnn-svg-icons";
import Localization from "localization";
import PackageCardOverlay from "./PackageCardOverlay";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
import util from "utils";

class ImportModal extends Component {
    constructor() {
        super();
        this.state = {
            wizardStep: 0
        };
    }

    componentWillMount() {
        const { props } = this;
        props.dispatch(ImportExportActions.getImportPackages());
    }

    componentWillReceiveProps(props) {
        this.setState({
            wizardStep: props.wizardStep
        });
    }

    goToStep(wizardStep) {
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
        props.dispatch(ImportExportActions.selectPackage());
        props.dispatch(ImportExportActions.importWizardGoToSetp(0));
    }

    selectPackage(packageId) {
        const { props } = this;
        props.dispatch(ImportExportActions.selectPackage(packageId));
    }

    cancelImport() {
        const { props } = this;
        this.goToStep(0);        
        props.onCancel();
    }

    onAnalyze() {
        const { props } = this;
        if (props.selectedPackageId) {
            props.dispatch(ImportExportActions.importWizardGoToSetp(1, () => {
                props.dispatch(ImportExportActions.verifyImportPackage(props.selectedPackageId));
            }));
        }
        else {
            util.utilities.notifyError(Localization.get("SelectException"));
        }
    }

    /* eslint-disable react/no-danger */
    renderTemplateThumbnail(imgData) {
        if (imgData) {
            return <img className="package-image" src={"data:image/jpeg;base64," + btoa(imgData)}></img>;
        }
        else {
            return <div className="template-icon" dangerouslySetInnerHTML={{ __html: TemplateIcon }}></div>;
        }
    }

    renderPackagesList() {
        const { props } = this;
        if (props.importPackages && props.importPackages.length > 0) {
            return props.importPackages.map((pkg, index) => {
                return (
                    <div className="package-card">
                        <div id={"package-card-" + index}>
                            {this.renderTemplateThumbnail()}
                            <div className="package-name">{pkg.Name}</div>
                            <div className="package-file">{pkg.FileName}</div>
                        </div>
                        <PackageCardOverlay selectPackage={this.selectPackage.bind(this, pkg.PackageId)} packageName={pkg.Name} packageDescription={pkg.Description} />
                    </div>
                );
            });
        }
        else return <div className="noPackages">{Localization.get("NoPackages")}</div>;
    }

    renderPackageVerification() {
        const { props } = this;
    }

    render() {
        const { props, state } = this;
        return (
            <div className={styles.importModal}>
                <div className="pageTitle">{Localization.get("SelectImportPackage.Header")}</div>
                <div className="pageDescription">{Localization.get("SelectImportPackage")}</div>
                <div className="packages-wrapper">
                    {state.wizardStep === 0 &&
                        <div className="packages">
                            {this.renderPackagesList()}
                        </div>
                    }
                    {state.wizardStep === 1 &&
                        <div className="packages">
                            {this.renderPackageVerification()}
                        </div>
                    }
                </div>
                <GridCell className="action-buttons">
                    <Button type="secondary" onClick={this.cancelImport.bind(this)}>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={this.onAnalyze.bind(this)}>{Localization.get("Continue")}</Button>
                </GridCell>
            </div>
        );
    }
}

ImportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    importPackages: PropTypes.array,
    selectedPackageId: PropTypes.string
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.importWizardStep,
        viewingLog: state.importExport.viewingLog,
        importPackages: state.importExport.importPackages,
        selectedPackageId: state.importExport.selectedPackageId
    };
}

export default connect(mapStateToProps)(ImportModal);