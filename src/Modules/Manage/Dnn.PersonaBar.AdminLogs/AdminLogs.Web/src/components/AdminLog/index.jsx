import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    log as LogActions
} from "../../actions";
import LogItemRow from "./LogItemRow";
import EmailPanel from "./EmailPanel";
import Checkbox from "dnn-checkbox";
import DropDown from "dnn-dropdown";
import Pager from "dnn-pager";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import {
    createPortalOptions,
    createLogTypeOptions
} from "../../reducerHelpers";


let pageSizeOptions = [];

const pageSizeStyle = {
    float: "right",
    margin: "0 60px 0 0"
};

class AdminLogPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            logList: [],
            portalList: [],
            logTypeList: [],
            allRowIds: [],
            emailPanelOpen: false,
            currentPortal: "",
            currentPortalId: "",
            currentLogType: "",
            currentLogTypeKey: "",
            pageIndex: 0,
            pageSize: 10,
            selectedRowIds: [],
            excludedRowIds: [],
            totalCount: 0
        };
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(LogActions.getPortalList((dataPortal) => {
            let portalList = Object.assign([], dataPortal.Results);
            let currentPortalId = portalList[0].PortalID;
            let currentPortal = portalList[0].PortalName;
            this.setState({
                portalList,
                currentPortalId,
                currentPortal
            });
            props.dispatch(LogActions.getLogTypeList((dataLog) => {
                let logTypeList = Object.assign([], dataLog.Results);
                let currentLogType = logTypeList[0].LogTypeFriendlyName;
                let currentLogTypeKey = logTypeList[0].LogTypeKey;
                this.setState({
                    logTypeList,
                    currentLogType,
                    currentLogTypeKey
                });
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            }));
        }));

        pageSizeOptions = [];
        pageSizeOptions.push({ "value": 10, "label": "10 entries per page" });
        pageSizeOptions.push({ "value": 25, "label": "25 entries per page" });
        pageSizeOptions.push({ "value": 50, "label": "50 entries per page" });
        pageSizeOptions.push({ "value": 100, "label": "100 entries per page" });
        pageSizeOptions.push({ "value": 250, "label": "250 entries per page" });
    }

    getNextPage() {
        const {state} = this;
        if (state.currentPortalId === -1 || state.currentPortalId === "-1") {
            return {
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
        else {
            return {
                portalId: state.currentPortalId,
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
    }

    onSelectAll() {
        const {props} = this;
        if (props.excludedRowIds.length == 0) {
            props.dispatch(LogActions.deselectAll());
        }
        else {
            props.dispatch(LogActions.selectAll());
        }
    }

    onClearLog() {
        const {props} = this;
        util.utilities.confirm(resx.get("ClearLog"), resx.get("yes"), resx.get("no"), () => {
            let getNextPageParam = this.getNextPage();

            props.dispatch(LogActions.clearLog(() => {
                props.dispatch(LogActions.getLogList(getNextPageParam));
            }));
        });
    }

    onDeleteLogItems() {
        const {props, state} = this;
        if (props.selectedRowIds.length > 0) {
            util.utilities.confirm(resx.get("LogDeleteWarning"), resx.get("yes"), resx.get("no"), () => {
                if (props.excludedRowIds.length == 0 && this.isLastPage()) {
                    this.setState({
                        pageIndex: state.pageIndex > 0 ? state.pageIndex - 1 : 0
                    }, () => {
                        let getNextPageParam = this.getNextPage();
                        props.dispatch(LogActions.deleteLogItems(props.selectedRowIds, () => {
                            props.dispatch(LogActions.getLogList(getNextPageParam));
                        }));
                    });
                }
                else {
                    let getNextPageParam = this.getNextPage();
                    props.dispatch(LogActions.deleteLogItems(props.selectedRowIds, () => {
                        props.dispatch(LogActions.getLogList(getNextPageParam));
                    }));
                }
            });
        }
        else {
            util.utilities.notify(resx.get("SelectException"));
        }
    }

    onEmailLogItems() {
        const {props} = this;
        if (props.selectedRowIds.length > 0) {
            props.dispatch(LogActions.emailLogItems(props.selectedRowIds));
        }
        else {
            alert((resx.get("SelectException")));
        }
    }

    toggleEmailPanel() {
        this.setState({
            emailPanelOpen: !this.state.emailPanelOpen
        });
    }

    onSelectPortal(option) {
        const {props, state} = this;
        if (option.value !== state.currentPortalId) {
            this.setState({
                currentPortal: option.label,
                currentPortalId: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
        }
    }

    onSelectLogType(option) {
        const {props, state} = this;
        if (option.value !== state.currentLogTypeKey) {
            this.setState({
                currentLogType: option.label,
                currentLogTypeKey: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
        }
    }

    renderedPortalList() {
        const {props} = this;
        let portals = props.portalList.map((term, index) => {
            return (
                <li onClick={this.onSelectPortal.bind(this, term.PortalID, term.PortalName)} value={term.PortalID}>{term.PortalName}</li>
            );
        });
        //portals.unshift(<li onClick={this.onSelectPortal.bind(this, "-1", "All Sites") } value={-1}>All Sites</li>);
        return <ul className="site-group-filter">{portals}
        </ul>;
    }

    renderedLogTypeList() {
        const {props} = this;
        let logTypes = props.logTypeList.map((term, index) => {
            return (
                <li onClick={this.onSelectLogType.bind(this, term.LogTypeKey, term.LogTypeFriendlyName)}>{term.LogTypeFriendlyName}</li>
            );
        });
        //logTypes.unshift(<li onClick={this.onSelectLogType.bind(this, "*", "All Types") }>All Types</li>);
        return <ul className="site-group-filter">{logTypes}
        </ul>;
    }

    renderLogListHeader() {
        const {props, state} = this;
        if (!props.excludedRowIds) {
            return;
        }

        const tableFields = [
            { "name": "", "id": "LogTypeCSSClass" },
            { "name": resx.get("Date"), "id": "LogCreateDate" },
            { "name": resx.get("Type"), "id": "LogTypeFriendlyName" },
            { "name": resx.get("Username"), "id": "LogUserName" },
            { "name": resx.get("Portal"), "id": "LogPortalName" },
            { "name": resx.get("Summary"), "id": "Summary" }
        ];

        let tableHeaders = tableFields.map((field) => {
            let className = "logHeader logHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });

        const isDeselectState = props.excludedRowIds.length == 0 && props.excludedRowIds.length || !props.excludedRowIds.length == 0 && props.selectedRowIds.length;
        const checkboxClassName = "checkbox" + (isDeselectState ? " deselect-state" : "");
        tableHeaders.unshift(<div key={"selector" + "999999"} className="logHeader logHeader-Checkbox" data-index="0">
            <div className={checkboxClassName}>
                <Checkbox value={props.excludedRowIds.length === 0 && props.selectedRowIds.length > 0 || isDeselectState} onChange={this.onSelectAll.bind(this)} />
                <label htmlFor="selectAll"></label>
            </div>
        </div>);

        return <div className="logHeader-wrapper">{tableHeaders}</div>;
    }

    /* eslint-disable react/no-danger */
    renderedLogList() {
        const {props} = this;
        return props.logList.map((term, index) => {
            return (
                <LogItemRow
                    cssClass={term.LogTypeCSSClass}
                    logId={term.LogGUID}
                    allRowIds={this.props.logList.map((row) => row.LogGUID)}
                    typeName={term.LogTypeFriendlyName}
                    createDate={term.LogCreateDate}
                    userName={term.LogUserName}
                    portalName={term.LogPortalName}
                    summary={term.Summary}
                    index={index}
                    key={"logTerm-" + index}
                    closeOnClick={true}>
                    <div className="log-detail" dangerouslySetInnerHTML={{ __html: term.LogProperties }}></div>
                </LogItemRow>
            );
        });
    }

    isLastPage() {
        const {props, state} = this;
        let total = Math.ceil(props.totalCount / state.pageSize);
        let current = state.pageIndex * state.pageSize / state.pageSize + 1;
        return total === current ? true : false;
    }

    onPageChange(currentPage, pageSize) {
        let {state, props} = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            let getNextPageParam = this.getNextPage();
            props.dispatch(LogActions.getLogList(getNextPageParam));
        });
    }

    renderPager() {
        const {props, state} = this;
        return (
            <div className="logPager">
                <Pager
                    showStartEndButtons={false}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalCount}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    />
            </div>
        );
    }

    renderedLogLegend() {
        const legendItems = [
            { "name": resx.get("ExceptionCode"), "id": "Exception" },
            { "name": resx.get("ItemCreatedCode"), "id": "ItemCreated" },
            { "name": resx.get("ItemUpdatedCode"), "id": "ItemUpdated" },
            { "name": resx.get("ItemDeletedCode"), "id": "ItemDeleted" },
            { "name": resx.get("SuccessCode"), "id": "OperationSuccess" },
            { "name": resx.get("FailureCode"), "id": "OperationFailure" },
            { "name": resx.get("AdminOpCode"), "id": "GeneralAdminOperation" },
            { "name": resx.get("AdminAlertCode"), "id": "AdminAlert" },
            { "name": resx.get("HostAlertCode"), "id": "HostAlert" },
            { "name": resx.get("SecurityException"), "id": "SecurityException" }
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

    render() {
        const {props, state} = this;
        let portalOptions = createPortalOptions(state.portalList);
        let logTypeOptions = createLogTypeOptions(state.logTypeList);
        return (
            state.portalList.length > 0 &&
            state.logTypeList.length > 0 &&
            <div>
                <div className="toolbar">
                    <div className="sitegroup-filter-container">
                        <DropDown
                            value={state.currentPortalId}
                            fixedHeight={200}
                            style={{ width: "100%" }}
                            options={portalOptions}
                            withBorder={false}
                            onSelect={this.onSelectPortal.bind(this)}
                            />
                    </div>
                    <div className="sitegroup-filter-container">
                        <DropDown
                            value={state.currentLogTypeKey}
                            fixedHeight={200}
                            style={{ width: "100%" }}
                            options={logTypeOptions}
                            withBorder={false}
                            onSelect={this.onSelectLogType.bind(this)}
                            />
                    </div>
                    <div className="toolbar-button toolbar-button-actions">
                        <span onClick={this.toggleEmailPanel.bind(this)}>{resx.get("btnEmail")} </span>
                        <div className="collapsible-content">
                            <EmailPanel
                                fixedHeight={370}
                                isOpened={state.emailPanelOpen}
                                logIds={props.selectedRowIds}
                                onCloseEmailPanel={this.toggleEmailPanel.bind(this)}>
                            </EmailPanel>
                        </div>
                    </div>
                    <div className="toolbar-button toolbar-button-actions" onClick={this.onDeleteLogItems.bind(this)}>{resx.get("btnDelete")} </div>
                    <div className="toolbar-button toolbar-button-actions" onClick={this.onClearLog.bind(this)}>{resx.get("btnClear")} </div>
                </div>
                <div className="logContainer">
                    <div className="logContainerBox">
                        {this.renderLogListHeader()}
                        {this.renderedLogList()}
                    </div>
                </div>
                {this.renderPager()}
                {this.renderedLogLegend()}
            </div>
        );
    }
}

AdminLogPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    logList: PropTypes.array,
    logTypeList: PropTypes.array,
    portalList: PropTypes.array,
    selectedRowIds: PropTypes.array.isRequired,
    allRowIds: PropTypes.array.isRequired,
    excludedRowIds: PropTypes.array.isRequired,
    totalCount: PropTypes.number
};

function mapStateToProps(state) {
    return {
        logList: state.log.logList,
        logTypeList: state.log.logTypeList,
        portalList: state.log.portalList,
        tabIndex: state.pagination.tabIndex,
        selectedRowIds: state.log.selectedRowIds,
        excludedRowIds: state.log.excludedRowIds,
        totalCount: state.log.totalCount
    };
}

export default connect(mapStateToProps)(AdminLogPanelBody);