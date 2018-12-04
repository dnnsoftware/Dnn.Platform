import React, {Component,  } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import Localization from "../../../localization";
import util from "../../../utils";
import OverflowText from "dnn-text-overflow-wrapper";

export default class BackupGrid extends Component {

    getBackUpsGridRows() {
        if (this.props.backups && this.props.backups.length > 0) {
            const rows = this.props.backups.map((field, i) => {
                return <div className="row" key={i}>
                    <GridCell columnSize={45}><OverflowText text={field.name} maxWidth={290} /></GridCell>
                    <GridCell columnSize={15}>{util.formatDateNoTime(field.startDate)}</GridCell>
                    <GridCell columnSize={15}>{util.formatDateNoTime(field.finishDate)}</GridCell>
                    <GridCell columnSize={10}>{util.formatNumeric(field.size)}</GridCell>
                    <GridCell columnSize={15}>{field.backupType}</GridCell>
                </div>;
            });
            return rows;
        }
        return <div className="row">
            <GridCell className="alert-message" columnSize={100}>{Localization.get("NoBackups")}</GridCell>
        </div>;
    }

    render() {
        const rows = this.getBackUpsGridRows();

        return <div className="grid">
            <div className="row header">
                <GridCell columnSize={45}>{Localization.get("BackupName")}</GridCell>
                <GridCell columnSize={15}>{Localization.get("BackupStarted")}</GridCell>
                <GridCell columnSize={15}>{Localization.get("BackupFinished")}</GridCell>
                <GridCell columnSize={10}>{Localization.get("BackupSize")}</GridCell>
                <GridCell columnSize={15}>{Localization.get("BackupType")}</GridCell>
            </div>
            {rows}
        </div>;
    }
}

BackupGrid.propTypes = {   
    backups: PropTypes.array
};