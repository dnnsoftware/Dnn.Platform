import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
} from "../../actions";
import "./style.less";
import util from "../../utils";
import Localization from "localization";
import JobRow from "./JobRow";
import FiltersBar from "./FiltersBar";
import JobDetails from "./JobDetails";
import { Dropdown, Pager, Button, Label, GridCell } from "@dnnsoftware/dnn-react-common";

class DashboardPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            jobs: [],
            pageIndex: 0,
            pageSize: 10,
            filter: null,
            keyword: "",
            currentJobId: null
        };
    }

    componentDidMount() {
        const { props } = this;
        props.dispatch(ImportExportActions.getPortals((data) => {
            if (data.TotalResults === 1) {
                props.dispatch(ImportExportActions.siteSelected(data.Results[0].PortalID, data.Results[0].PortalName, () => {
                    props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(data.Results[0].PortalID), (data) => {
                        if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                            this.addInterval(props);
                        }
                        else {
                            clearInterval(this.jobListTimeout);
                        }
                    }));
                }));
            }
            else {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                    if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        this.addInterval(props);
                    }
                    else {
                        clearInterval(this.jobListTimeout);
                    }
                }));
            }
        }));
    }

    componentDidUpdate() {
        const { props } = this;
        if (this.state.currentJobId !== props.currentJobId) {
            this.setState({
                currentJobId: props.currentJobId
            });
        }
    }

    componentWillUnmount() {
        clearInterval(this.jobListTimeout);
    }

    addInterval(props) {
        clearInterval(this.jobListTimeout);
        const persistedSettings = util.utilities.persistent.load();
        this.jobListTimeout = setInterval(() => {
            if (persistedSettings.expandPersonaBar && persistedSettings.activeIdentifier === "Dnn.SiteImportExport") {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(), (data) => {
                    if (!data.Jobs || !data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        clearInterval(this.jobListTimeout);
                        if (this.state.currentJobId) {
                            props.dispatch(ImportExportActions.getJobDetails(this.state.currentJobId));
                        }
                    }
                    else if (this.state.currentJobId && data.Jobs.find(j => j.JobId === this.state.currentJobId && j.Status < 2 && !j.Cancelled)) {
                        props.dispatch(ImportExportActions.getJobDetails(this.state.currentJobId));
                    }
                }));
            }
        }, 5000);
    }

    uncollapse(id) {
        this.props.dispatch(ImportExportActions.jobSelected(id));
    }

    collapse() {
        if (this.props.currentJobId !== "") {
            this.props.dispatch(ImportExportActions.jobSelected());
        }
    }

    toggle(id) {
        if (id !== "") {
            this.uncollapse(id);
        }
        else {
            this.collapse();
        }
    }

    isLastPageOnlyJob(jobId) {
        const { props, state } = this;
        let total = Math.ceil(props.totalJobs / state.pageSize);
        let current = state.pageIndex * state.pageSize / state.pageSize + 1;
        let isLastPage = total === current ? true : false;
        if (isLastPage && props.jobs.length === 1 && props.jobs[0].JobId === jobId) {
            return true;
        }
        else return false;
    }

    getNextPage(portalId) {
        const { state, props } = this;
        return {
            portal: portalId === undefined ? props.portalId : portalId,
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize,
            jobType: state.filter === -1 ? null : state.filter,
            keywords: state.keyword
        };
    }

    getPortalOptions() {
        const { props } = this;
        let options = [];
        if (props.portals) {
            options = props.portals.map((item) => {
                return {
                    label: item.PortalName,
                    value: item.PortalID
                };
            });
            if (options.length > 1) {
                options.unshift({ "label": Localization.get("AllSites"), "value": -1 });
            }
        }
        return options;
    }

    onPortalChange(option) {
        const { props } = this;
        if (option.value !== props.portalId) {
            this.setState({
                pageIndex: 0
            }, () => {
                props.dispatch(ImportExportActions.siteSelected(option.value, option.label));
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(option.value), (data) => {
                    if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        this.addInterval(props);
                    }
                    else {
                        clearInterval(this.jobListTimeout);
                    }
                }));
            });
        }
    }

    onImportData() {
        const { props } = this;
        props.selectPanel(2);
    }

    onExportData() {
        const { props } = this;
        props.selectPanel(1);
    }

    onPageChange(currentPage, pageSize) {
        let { state, props } = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            props.dispatch(ImportExportActions.jobSelected(null, () => {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                    if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        this.addInterval(props);
                    }
                    else {
                        clearInterval(this.jobListTimeout);
                    }
                }));
            }));
        });
    }

    onFilterChanged(filter) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            filter: filter.value
        }, () => {
            props.dispatch(ImportExportActions.jobSelected(null, () => {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                    if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        this.addInterval(props);
                    }
                    else {
                        clearInterval(this.jobListTimeout);
                    }
                }));
            }));
        });
    }

    onKeywordChanged(keyword) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            keyword: keyword
        }, () => {
            props.dispatch(ImportExportActions.jobSelected(null, () => {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                    if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                        this.addInterval(props);
                    }
                    else {
                        clearInterval(this.jobListTimeout);
                    }
                }));
            }));
        });
    }

    cancelJob(jobId) {
        const { props } = this;
        util.utilities.confirm(Localization.get("CancelJobMessage"),
            Localization.get("ConfirmCancel"),
            Localization.get("No"),
            () => {
                props.dispatch(ImportExportActions.cancelJob(jobId, () => {
                    util.utilities.notify(Localization.get("JobCancelled"));
                    props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                        if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                            this.addInterval(props);
                        }
                        else {
                            clearInterval(this.jobListTimeout);
                        }
                    }));
                }, () => {
                    util.utilities.notifyError(Localization.get("JobCancel.ErrorMessage"));
                }));
            }
        );
    }

    deleteJob(jobId) {
        const { props, state } = this;
        util.utilities.confirm(Localization.get("DeleteJobMessage"),
            Localization.get("ConfirmDelete"),
            Localization.get("No"),
            () => {
                if (this.isLastPageOnlyJob(jobId)) {
                    this.setState({
                        pageIndex: state.pageIndex > 0 ? state.pageIndex - 1 : 0
                    }, () => {
                        props.dispatch(ImportExportActions.deleteJob(jobId, () => {
                            util.utilities.notify(Localization.get("JobDeleted"));
                            this.collapse();
                            props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                                if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                                    this.addInterval(props);
                                }
                                else {
                                    clearInterval(this.jobListTimeout);
                                }
                            }));
                        }, () => {
                            util.utilities.notifyError(Localization.get("JobDelete.ErrorMessage"));
                        }));
                    });
                }
                else {
                    props.dispatch(ImportExportActions.deleteJob(jobId, () => {
                        util.utilities.notify(Localization.get("JobDeleted"));
                        this.collapse();
                        props.dispatch(ImportExportActions.getAllJobs(this.getNextPage(props.portalId), (data) => {
                            if (data.Jobs && data.Jobs.find(j => j.Status < 2 && !j.Cancelled)) {
                                this.addInterval(props);
                            }
                            else {
                                clearInterval(this.jobListTimeout);
                            }
                        }));
                    }, () => {
                        util.utilities.notifyError(Localization.get("JobDelete.ErrorMessage"));
                    }));
                }
            }
        );
    }

    renderTopPane() {
        const { props } = this;
        return <div className="top-panel">
            <GridCell columnSize={100} >
                <div className="site-selection">
                    <Dropdown
                        enabled={props.portals.length > 1 ? true : false}
                        options={this.getPortalOptions()}
                        autoHide={false}
                        value={props.portalId}
                        onSelect={this.onPortalChange.bind(this)}
                        prependWith={Localization.get("ShowSiteLabel")}
                    />
                </div>
                <div className="last-actions">
                    <div className="action-labels">
                        <Label
                            labelType="block"
                            label={Localization.get("LastImport")} />
                        <Label
                            labelType="block"
                            label={Localization.get("LastExport")} />
                    </div>
                    <div className="action-dates">
                        <div>{props.lastImportTime || Localization.get("EmptyDateTime")}</div>
                        <div>{props.lastExportTime || Localization.get("EmptyDateTime")}</div>
                    </div>
                </div>
                <div className="action-buttons">
                    <Button
                        disabled={props.portalId < 0}
                        className="action-button"
                        type="secondary"
                        onClick={this.onExportData.bind(this)}>
                        {Localization.get("ExportButton")}
                    </Button>
                    <Button
                        disabled={props.portalId < 0}
                        className="action-button"
                        type="secondary"
                        onClick={this.onImportData.bind(this)}>
                        {Localization.get("ImportButton")}
                    </Button>
                </div>
            </GridCell>
        </div>;
    }

    renderJobListHeader() {
        const tableFields = [
            { "name": "", "id": "Indicator" },
            { "name": Localization.get("JobDate.Header"), "id": "JobDate" },
            { "name": Localization.get("JobType.Header"), "id": "JobType" },
            { "name": Localization.get("JobUser.Header"), "id": "JobUser" },
            { "name": Localization.get("JobPortal.Header"), "id": "JobPortal" },
            { "name": Localization.get("JobStatus.Header"), "id": "JobStatus" },
            { "name": "", "id": "Arrow" }
        ];

        let tableHeaders = tableFields.map((field, i) => {
            let className = "jobHeader jobHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="jobHeader-wrapper">{tableHeaders}</div>;
    }

    renderedJobList() {
        const { props } = this;
        if (props.portals.length > 0 && props.jobs && props.jobs.length > 0) {
            return props.jobs.map((job, index) => {
                return (
                    <JobRow
                        jobId={job.JobId}
                        jobType={job.JobType}
                        jobDate={job.CreatedOnString}
                        jobUser={job.User}
                        jobPortal={props.portals.find(p => p.PortalID === job.PortalId) ? props.portals.find(p => p.PortalID === job.PortalId).PortalName : Localization.get("DeletedPortal")}
                        jobStatus={job.Status}
                        jobCancelled={job.Cancelled}
                        index={index}
                        key={"jobTerm-" + index}
                        closeOnClick={true}
                        openId={props.currentJobId}
                        OpenCollapse={this.toggle.bind(this, job.JobId)}
                        Collapse={this.collapse.bind(this)}>
                        <JobDetails
                            jobId={job.JobId}
                            cancelled={job.Cancelled}
                            Collapse={this.collapse.bind(this)}
                            openId={props.currentJobId}
                            cancelJob={this.cancelJob.bind(this)}
                            deleteJob={this.deleteJob.bind(this)} />
                    </JobRow>
                );
            });
        }
        else return <GridCell className="no-jobs">{Localization.get("NoJobs")}</GridCell>;
    }

    renderPager() {
        const { props, state } = this;
        return (
            <div className="logPager">
                {props.jobs && <Pager
                    showStartEndButtons={true}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalJobs}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture()}
                />}
            </div>
        );
    }

    renderedLegend() {
        const legendItems = [
            { "name": Localization.get("LegendExport"), "id": "legend-export" },
            { "name": Localization.get("LegendImport"), "id": "legend-import" }
        ];
        let legend = legendItems.map((item, i) => {
            return <div className="logLegend-item" key={i}>
                <div className={item.id}>
                    <span></span>
                </div>
                <div>
                    <span>{item.name}</span>
                </div>
            </div>;
        });

        return <div className="logLegend-wrapper">{legend}</div>;
    }

    render() {
        const { props } = this;
        return (
            <div>
                {props.portals.length > 0 && <div>
                    {this.renderTopPane()}
                    <div className="section-title">{Localization.get("LogSection")}</div>
                    <FiltersBar onFilterChanged={this.onFilterChanged.bind(this)}
                        onKeywordChanged={this.onKeywordChanged.bind(this)}
                    />
                    <div className="logContainer">
                        <div className="logContainerBox">
                            {this.renderJobListHeader()}
                            {this.renderedJobList()}
                        </div>
                    </div>
                    {this.renderPager()}
                    {this.renderedLegend()}
                </div>
                }
            </div>
        );
    }
}

DashboardPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    jobs: PropTypes.array,
    portals: PropTypes.array,
    totalJobs: PropTypes.number,
    portalName: PropTypes.string,
    selectPanel: PropTypes.func,
    portalId: PropTypes.number,
    lastExportTime: PropTypes.string,
    lastImportTime: PropTypes.string,
    currentJobId: PropTypes.number
};

DashboardPanelBody.defaultProps = {
    totalJobs: 0
};

function mapStateToProps(state) {
    return {
        jobs: state.importExport.jobs,
        portalId: state.importExport.portalId,
        portals: state.importExport.portals,
        totalJobs: state.importExport.totalJobs,
        portalName: state.importExport.portalName,
        lastExportTime: state.importExport.lastExportTime,
        lastImportTime: state.importExport.lastImportTime,
        currentJobId: state.importExport.currentJobId
    };
}

export default connect(mapStateToProps)(DashboardPanelBody);