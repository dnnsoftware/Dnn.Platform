import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
} from "../../actions";
import DropDown from "dnn-dropdown";
import Pager from "dnn-pager";
import Button from "dnn-button";
import Label from "dnn-label";
import "./style.less";
import util from "../../utils";
import Localization from "localization";
import JobRow from "./JobRow";
import FiltersBar from "./FiltersBar";
import JobDetails from "./JobDetails";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";

let isHost = false;
let currentPortalId = -1;

/*eslint-disable eqeqeq*/
class DashboardPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            jobs: [],
            portals: [],
            portalId: -1,
            pageIndex: 0,
            pageSize: 10,
            filter: null,
            keyword: "",
            openId: ""
        };
        isHost = util.settings.isHost;
        currentPortalId = util.settings.portalId;
    }

    componentWillMount() {
        const { state, props } = this;
        this.setState({
            portalId: props.portalId || currentPortalId
        }, () => {
            if (isHost) {
                props.dispatch(ImportExportActions.getPortals(() => {
                }),
                    props.dispatch(ImportExportActions.getAllJobs(this.getNextPage()))
                );
            }
        });
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
        else {
            this.collapse();
        }
    }

    getNextPage() {
        const { state } = this;
        return {
            portalId: state.portalId,
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize
        };
    }

    getPortalOptions() {
        const { state, props } = this;
        let options = [];
        if (props.portals !== undefined) {
            options = props.portals.map((item) => {
                return {
                    label: item.PortalName,
                    value: item.PortalID
                };
            });
        }
        return options;
    }

    onPortalChange(option) {
        const { props, state } = this;
        if (option.value !== state.portalId) {
            this.setState({
                portalId: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(ImportExportActions.getAllJobs(this.getNextPage()));
            });
        }
    }

    onImportData() {
        const { props } = this;
        props.dispatch(ImportExportActions.import());
    }

    onExportData() {
        const { props } = this;
        props.dispatch(ImportExportActions.export());
    }

    renderJobListHeader() {
        const { props } = this;
        const tableFields = [
            { "name": "", "id": "Indicator" },
            { "name": Localization.get("JobDate.Header"), "id": "JobDate" },
            { "name": Localization.get("JobType.Header"), "id": "JobType" },
            { "name": Localization.get("JobUser.Header"), "id": "JobUser" },
            { "name": Localization.get("JobPortal.Header"), "id": "JobPortal" },
            { "name": "", "id": "Arrow" }
        ];

        let tableHeaders = tableFields.map((field) => {
            let className = "jobHeader jobHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="jobHeader-wrapper">{tableHeaders}</div>;
    }

    /* eslint-disable react/no-danger */
    renderedJobList() {
        const { props } = this;
        let i = 0;
        return props.jobs.map((job, index) => {
            let id = "row-" + i++;
            return (
                <JobRow
                    jobId={job.JobId}
                    jobType={job.JobType}
                    jobDate={job.CreatedOn}
                    jobUser={job.User}
                    jobPortal={job.PortalId}
                    index={index}
                    key={"jobTerm-" + index}
                    closeOnClick={true}
                    openId={this.state.openId}
                    OpenCollapse={this.toggle.bind(this)}
                    Collapse={this.collapse.bind(this)}
                    id={id}>
                    <JobDetails
                        jobId={job.JobId}
                        Collapse={this.collapse.bind(this)}
                        id={id}
                        openId={this.state.openId} />
                </JobRow>
            );
        });
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
            props.dispatch(ImportExportActions.getAllJobs(this.getNextPage()));
        });
    }

    renderPager() {
        const { props, state } = this;
        return (
            <div className="logPager">
                <Pager
                    showStartEndButtons={false}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={80}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture()}
                />
            </div>
        );
    }

    renderedLegend() {
        const legendItems = [
            { "name": Localization.get("LegendExport"), "id": "legend-export" },
            { "name": Localization.get("LegendImport"), "id": "legend-import" }
        ];
        let legend = legendItems.map((item) => {
            return <div className="logLegend-item">
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

    onFilterChanged(filter) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            filter: filter.value
        }, () => {
            props.dispatch(ImportExportActions.getAllJobs(this.getNextPage()));
        });
    }

    onKeywordChanged(keyword) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            keyword: keyword
        }, () => {
            props.dispatch(ImportExportActions.getAllJobs(this.getNextPage()));
        });
    }

    render() {
        const { props, state } = this;
        return (
            <div>
                <div className="top-panel">
                    <div className="site-selection">
                        <DropDown
                            options={this.getPortalOptions()}
                            value={state.portalId}
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
                            <Label
                                labelType="block"
                                label={Localization.get("LastUpdate")} />
                        </div>
                        <div className="action-dates">
                            <div>1/31/2017 12:45 PM</div>
                            <div>1/31/2017 12:45 PM</div>
                            <div>1/31/2017 12:45 PM</div>
                        </div>
                    </div>
                    <div className="action-buttons">
                        <Button
                            className="action-button"
                            type="secondary"
                            onClick={this.onExportData.bind(this)}>
                            {Localization.get("ExportButton")}
                        </Button>
                        <Button
                            className="action-button"
                            type="secondary"
                            onClick={this.onImportData.bind(this)}>
                            {Localization.get("ImportButton")}
                        </Button>
                    </div>
                </div>
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
        );
    }
}

DashboardPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    jobs: PropTypes.array,
    portals: PropTypes.array
};

function mapStateToProps(state) {
    return {
        jobs: state.importExport.jobs,
        portals: state.importExport.portals
    };
}

export default connect(mapStateToProps)(DashboardPanelBody);