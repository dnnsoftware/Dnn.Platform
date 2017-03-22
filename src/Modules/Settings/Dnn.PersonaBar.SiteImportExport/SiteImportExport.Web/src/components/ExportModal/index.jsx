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

    cancelExport() {
        const { props } = this;
        this.goToStep(0);
        props.onCancel();
    }

    render() {
        const { props } = this;
        const { wizardStep } = props;
        return (
            <div className={styles.exportModal}>
                <div>export</div>
            </div>
        );
    }
}

ExportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    exportLogs: PropTypes.array,
    viewingLog: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.exportWizardStep,
        exportLogs: state.importExport.exportLogs,
        viewingLog: state.importExport.viewingLog
    };
}

export default connect(mapStateToProps)(ExportModal);