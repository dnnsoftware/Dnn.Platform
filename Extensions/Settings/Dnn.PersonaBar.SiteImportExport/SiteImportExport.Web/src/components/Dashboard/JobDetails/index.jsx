import React, { Component } from "react";
import PropTypes from "prop-types";
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
import itemsToExportService from "../../../services/itemsToExportService";

class JobDetails extends Component {
    constructor(props) {
        super();
        this.state = {
            cancelled: props.cancelled
        };
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        if (props.jobId) {
            props.dispatch(ImportExportActions.getJobDetails(props.jobId));
        }
    }

    UNSAFE_componentWillReceiveProps(props) {
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
            let detail = props.jobDetail.Summary.SummaryItems.find(c => c.Category.toUpperCase() === category.toUpperCase());
            if (detail) {
                if (detail.Completed || state.cancelled || props.jobDetail.Status !== 1 || detail.TotalItems === 0) {
                    return detail.ProcessedItems > detail.TotalItems ? detail.TotalItemsString : detail.ProcessedItemsString + " / " + detail.TotalItemsString;
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
        const { props, state } = this;
        if (props.jobDetail.Summary) {
            let users = props.jobDetail.Summary.SummaryItems.find(c => c.Category === "USERS");
            if (users && props.jobDetail.JobType === "Site Import") {
                let usersData = props.jobDetail.Summary.SummaryItems.find(c => c.Category === "USERS_DATA");
                if (users.TotalItems > 0 && !users.Completed && !state.cancelled) {
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={Localization.get("UsersStep1")}
                        />
                        <div className="import-summary-item users">
                            <div>
                                <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                                <div style={{ float: "right" }}>
                                    {users.ProcessedItemsString + " / " + users.TotalItemsString + " (" + (users.ProcessedItems / users.TotalItems * 100).toFixed(1) + "%)"}
                                </div>
                            </div>
                        </div>
                    </GridCell>;
                }
                else if (users.TotalItems > 0 && users.Completed && usersData.TotalItems === 0) {
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={Localization.get("UsersStep1")}
                        />
                        <div className="import-summary-item users">
                            {users.ProcessedItems > users.TotalItems ? users.TotalItemsString : users.ProcessedItemsString + " / " + users.TotalItemsString}
                        </div>
                    </GridCell>;
                }
                else if (usersData.TotalItems > 0 && !usersData.Completed && !state.cancelled) {
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={Localization.get("UsersStep2")}
                        />
                        <div className="import-summary-item users">
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
                        <div className="import-summary-item users">
                            {users.ProcessedItems > users.TotalItems ? users.TotalItemsString : users.ProcessedItemsString + " / " + users.TotalItemsString}
                        </div>
                    </GridCell>;
                }

            }
            else if (users && props.jobDetail.JobType === "Site Export") {
                if (users.TotalItems > 0 && !users.Completed && !state.cancelled) {
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={Localization.get("Users")}
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
                    return <GridCell>
                        <Label
                            labelType="inline"
                            label={Localization.get("Users")}
                        />
                        <div className="import-summary-item">
                            {users.ProcessedItems > users.TotalItems ? users.TotalItemsString : users.ProcessedItemsString + " / " + users.TotalItemsString}
                        </div>
                    </GridCell>;
                }
            }
            else {
                return <GridCell>
                    <Label
                        labelType="inline"
                        label={Localization.get("Users")}
                    />
                    <div className="import-summary-item">{"-"}</div>
                </GridCell>;
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
        const registeredItemsToExport = itemsToExportService.getRegisteredItemsToExport();
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
                                <div className="import-summary-item pagers">{this.getSummaryItem("Pages")}</div>
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
                                    label={Localization.get("Extensions")}
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
                            {registeredItemsToExport.map((item, i) =>
                                <GridCell key={i}>
                                    <Label
                                        labelType="inline"
                                        label={item.name}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem(item.category)}</div>
                                </GridCell>)
                            }
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
                                    {props.jobDetail.JobType.indexOf("Export") >= 0 ? Localization.get("CancelExport") : Localization.get("CancelImport")}
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
                    <div className="summary-title">{props.jobDetail.JobType.indexOf("Export") >= 0 ? Localization.get("ExportSummary") : Localization.get("ImportSummary")}</div>
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