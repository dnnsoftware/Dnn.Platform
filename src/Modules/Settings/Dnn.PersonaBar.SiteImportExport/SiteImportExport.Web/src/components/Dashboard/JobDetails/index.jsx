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
    constructor(props) {
        super();
        this.state = {
            cancelled: props.cancelled
        };
    }

    componentWillMount() {
        const { props } = this;
        if (props.jobId) {
            props.dispatch(ImportExportActions.getJobDetails(props.jobId));
        }
    }

    componentWillReceiveProps(props) {
        if (props.cancelled !== this.state.cancelled) {
            this.setState({
                cancelled: props.cancelled
            });
        }
    }

    /* eslint-disable react/no-danger */
    getSummaryItem(category) {
        const { props, state } = this;
        if (props.jobDetail.Summary) {
            let detail = props.jobDetail.Summary.SummaryItems.find(c => c.Category === category.toUpperCase());
            if (detail) {
                if (detail.ProcessedItems === detail.TotalItems || state.cancelled || props.jobDetail.Status !== 1) {
                    return detail.ProcessedItemsString + " / " + detail.TotalItemsString;
                }
                else {
                    return <div>
                        <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                        <div style={{ float: "right" }}>
                            {detail.ProcessedItemsString + " / " + detail.TotalItemsString + " (" + (detail.ProcessedItems / detail.TotalItems * 100).toFixed(1) + "%)"}
                        </div>
                    </div>;
                }
            }
            else return "-";
        }
        else {
            return "-";
        }
    }

    getUsersSummary() {
        const { props } = this;
        if (props.jobDetail.Summary) {
            let users = props.jobDetail.Summary.SummaryItems.find(c => c.Category === "USERS");
            let usersData = props.jobDetail.Summary.SummaryItems.find(c => c.Category === "USERS_DATA");
            if (users) {
                if (users.ProgressPercentage < 100 && users.TotalItems > 0) {
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={usersData && usersData.TotalItems > 0 ? Localization.get("UsersStep1") : Localization.get("Users")}
                        />
                        <div className="import-summary-item">
                            <div>
                                <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                                <div style={{ float: "right" }}>
                                    {users.ProcessedItemsString + " / " + users.TotalItemsString + " (" + (users.ProcessedItems / users.TotalItems * 100).toFixed(1) + "%)"}
                                </div>
                            </div>
                        </div>
                    </GridCell>;
                }
                else {
                    if (usersData && usersData.TotalItems > 0) {
                        if (usersData.ProgressPercentage < 100) {
                            return <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("UsersStep2")}
                                />
                                <div className="import-summary-item">
                                    <div>
                                        <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                                        <div style={{ float: "right" }}>
                                            {usersData.ProcessedItemsString + " / " + usersData.TotalItemsString + " (" + (usersData.ProcessedItems / usersData.TotalItems * 100).toFixed(1) + "%)"}
                                        </div>
                                    </div>
                                </div>
                            </GridCell>;
                        }
                        else {
                            return <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Users")}
                                />
                                <div className="import-summary-item">{usersData.ProcessedItemsString + " / " + usersData.TotalItemsString}</div>
                            </GridCell>;
                        }
                    }
                    else {
                        return <GridCell>
                            <Label
                                labelType="inline"
                                label={Localization.get("Users")}
                            />
                            <div className="import-summary-item">{users.ProcessedItemsString + " / " + users.TotalItemsString}</div>
                        </GridCell>;
                    }
                }
            }
        }
    }

    cancel(jobId) {
        this.props.cancelJob(jobId);
    }

    delete(jobId) {
        this.props.deleteJob(jobId);
    }

    renderExportSummary() {
        const { props, state } = this;
        return <div style={{ float: "left", width: "100%" }}>
            {props.jobDetail &&
                <div className="export-summary">
                    <GridCell className="export-site-container">
                        <div className="left-column">
                            {this.getUsersSummary()}
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
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeContent ? Localization.get("Yes") : Localization.get("No")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeProfileProperties")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeProfileProperties ? Localization.get("Yes") : Localization.get("No")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludePermissions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludePermissions ? Localization.get("Yes") : Localization.get("No")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeExtensions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeExtensions ? Localization.get("Yes") : Localization.get("No")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("IncludeDeletions")}
                                />
                                <div className="import-summary-item">{props.jobDetail.Summary.IncludeDeletions ? Localization.get("Yes") : Localization.get("No")}</div>
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
                                <div className="import-summary-item">{this.getSummaryItem("Packages")}</div>
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
                                <div className="import-summary-item">
                                    {props.jobDetail.Summary.ExportMode === 1 ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}
                                </div>
                            </GridCell>
                            <GridCell>
                                <div className="summary-note">
                                    <div className="note-title">{Localization.get("SummaryNoteTitle")}</div>
                                    <div className="note-description">{Localization.get("SummaryNoteDescription")}</div>
                                </div>
                            </GridCell>
                        </div>
                        <GridCell className="action-buttons">
                            {props.jobDetail.Status < 2 && !state.cancelled &&
                                <Button type="secondary" onClick={this.cancel.bind(this, props.jobId)}>
                                    {props.jobDetail.JobType.includes("Export") ? Localization.get("CancelExport") : Localization.get("CancelImport")}
                                </Button>
                            }
                            {(props.jobDetail.Status > 1 || state.cancelled) &&
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
    cancelled: PropTypes.bool,
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