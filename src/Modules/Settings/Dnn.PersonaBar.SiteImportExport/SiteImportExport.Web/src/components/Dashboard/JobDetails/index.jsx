import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import Label from "dnn-label";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import { CycleIcon } from "dnn-svg-icons";
import {
    importExport as ImportExportActions
} from "../../../actions";
import Localization from "localization";

class JobDetails extends Component {
    constructor() {
        super();
    }

    componentWillMount() {
        const { props } = this;
        if (props.jobId) {
            props.dispatch(ImportExportActions.getJobDetails(props.jobId));
        }
    }

    /* eslint-disable react/no-danger */
    getSummaryItem(category) {
        const { props } = this;
        if (props.jobDetail.Summary) {
            let detail = props.jobDetail.Summary.SummaryItems.find(c => c.Category === category.toUpperCase());
            if (detail) {
                if (detail.ProcessedItems === detail.TotalItems || props.jobDetail.Cancelled) {
                    return detail.ProcessedItems + " / " + detail.TotalItems;
                }
                else {
                    return <div>
                        <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                        <div style={{ float: "right" }}>{detail.ProcessedItems + " / " + detail.TotalItems + " (" + (detail.ProcessedItems / detail.TotalItems * 100).toFixed(1) + "%)"}</div>
                    </div>;
                }
            }
            else return "-";
        }
        else {
            return "-";
        }
    }

    cancel(jobId) {
        this.props.cancelJob(jobId);
    }

    delete(jobId) {
        this.props.deleteJob(jobId);
    }

    renderExportSummary() {
        const { props } = this;
        return <div style={{ float: "left", width: "100%" }}>
            {props.jobDetail &&
                <div className="export-summary">
                    <GridCell className="export-site-container">
                        <div className="left-column">
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Pages")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Pages")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Users")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Users")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Roles")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Roles")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Vocabularies")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Vocabularies")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("PageTemplates")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Templates")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeContent")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeContent ? props.jobDetail.Summary.IncludeContent.toString() : "-"}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeProfileProperties")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeProfileProperties.toString()}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludePermissions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludePermissions.toString()}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeExtensions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeExtensions.toString()}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeDeletions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeDeletions.toString()}</div>
                            </GridCell>
                        </div>
                        <div className="right-column">
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Name")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Name || "-"}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("FolderName")}
                                />
                                <div className="import-summary-item">{props.jobDetail.ExportFile}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ModulePackages")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Extensions")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Assets")}
                                />
                                <div className="import-summary-item">{this.getSummaryItem("Assets")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("TotalExportSize")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.ExportFileInfo ? props.jobDetail.Summary.ExportFileInfo.ExportSize : "-"}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ExportMode")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.ExportMode === 1 ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}</div>
                            </GridCell>
                            <GridCell>
                                <div className="summary-note">
                                    <div className="note-title">{Localization.get("SummaryNoteTitle")}</div>
                                    <div className="note-description">{Localization.get("SummaryNoteDescription")}</div>
                                </div>
                            </GridCell>
                        </div>
                        <GridCell className="action-buttons">
                            {props.jobDetail.Status < 2 && !props.jobDetail.Cancelled &&
                                <Button type="secondary" onClick={this.cancel.bind(this, props.jobId)}>
                                    {props.jobDetail.JobType.includes("Export") ? Localization.get("CancelExport") : Localization.get("CancelImport")}
                                </Button>
                            }
                            {(props.jobDetail.Status > 1 || props.jobDetail.Cancelled) &&
                                <Button type="secondary" onClick={this.delete.bind(this, props.jobId)}>{Localization.get("Delete")}</Button>
                            }
                        </GridCell>
                    </GridCell>
                </div>
            }
        </div>;
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        if (props.jobDetail !== undefined) {
            return (
                <div className="job-details">
                    <div className="summary-title">{props.jobDetail.JobType.includes("Export") ? Localization.get("ExportSummary") : Localization.get("ImportSummary")}</div>
                    {this.renderExportSummary()}
                </div>
            );
        }
        else return <div></div>;
    }
}

JobDetails.propTypes = {
    dispatch: PropTypes.func.isRequired,
    jobDetail: PropTypes.object,
    jobId: PropTypes.number,
    Collapse: PropTypes.func,
    cancelJob: PropTypes.func,
    deleteJob: PropTypes.func
};

function mapStateToProps(state) {
    return {
        jobDetail: state.importExport.job
    };
}

export default connect(mapStateToProps)(JobDetails);