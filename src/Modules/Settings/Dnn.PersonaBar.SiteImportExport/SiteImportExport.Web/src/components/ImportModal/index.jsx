import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
} from "../../actions";
import Localization from "localization";
import styles from "./style.less";

class ExportModal extends Component {
    constructor() {
        super();
        this.state = {
            wizardStep: 0
        };
    }

    goToStep(wizardStep) {
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
    }

    cancelImport() {
        const { props } = this;
        this.goToStep(0);
        props.onCancel();
    }

    render() {
        const { props } = this;
        const { wizardStep } = props;
        return (
            <div className={styles.importModal}>
                <div>import</div>
            </div>
        );
    }
}

ExportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    importLogs: PropTypes.array,
    viewingLog: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.exportWizardStep,
        importLogs: state.importExport.importLogs,
        viewingLog: state.importExport.viewingLog
    };
}

export default connect(mapStateToProps)(ExportModal);