import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    task as TaskActions
} from "../../actions";
import TaskHistoryItemRow from "./taskHistoryItemRow";
import "./style.less";
import { Pager } from "@dnnsoftware/dnn-react-common";
import resx from "../../resources";

/*eslint-disable quotes*/
const svgIcon = require(`!raw-loader!./../svg/history.svg`);

let pageSizeOptions = [];
let tableFields = [];

class HistoryPanelBody extends Component {
    constructor(props) {
        super();
        this.state = {
            taskHistoryList: [],
            scheduleId: -1,
            pageIndex: 0,
            pageSize: props.pageSize ? props.pageSize : 10,
            totalCount: 0
        };
    }

    UNSAFE_componentWillMount() {
        const {props, state} = this;
        props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: state.pageIndex, pageSize: state.pageSize }));

        tableFields = [];
        tableFields.push({ "name": resx.get("DescriptionColumn"), "id": "LogNotes" });
        tableFields.push({ "name": resx.get("RanOnServerColumn"), "id": "Server" });
        tableFields.push({ "name": resx.get("DurationColumn"), "id": "ElapsedTime" });
        tableFields.push({ "name": resx.get("SucceededColumn"), "id": "Succeeded" });
        tableFields.push({ "name": resx.get("StartEndColumn"), "id": "StartEnd" });

        pageSizeOptions = [];
        pageSizeOptions.push({ "value": "10", "label": "10 entries per page" });
        pageSizeOptions.push({ "value": "25", "label": "25 entries per page" });
        pageSizeOptions.push({ "value": "50", "label": "50 entries per page" });
        pageSizeOptions.push({ "value": "100", "label": "100 entries per page" });
        pageSizeOptions.push({ "value": "250", "label": "250 entries per page" });
    }

    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
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
            props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: state.pageIndex, pageSize: state.pageSize }));
        });
    }

    renderedHistoryListHeader() {
        let tableHeaders = tableFields.map((field, index) => {
            let className = "historyHeader historyHeader-" + field.id;
            return <div className={className} key={"historyHeader-" + index}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="historyHeader-wrapper">{tableHeaders}</div>;
    }

    renderPager() {
        const {props, state} = this;
        return (
            <div className="taskHistoryList-pager">
                <Pager
                    showStartEndButtons={true}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalCount || 0}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={resx.get("pageSizeOption")}
                    summaryText={resx.get("pagerSummary")}
                />
            </div>
        );
    }

    /* eslint-disable react/no-danger */
    renderedHistoryList() {
        const {props} = this;
        if (props.taskHistoryList) {
            return props.taskHistoryList.map((term, index) => {
                return (
                    <TaskHistoryItemRow
                        friendlyName={term.FriendlyName}
                        logNotes={term.LogNotes}
                        server={term.Server}
                        elapsedTime={term.ElapsedTime}
                        succeeded={term.Succeeded}
                        startDate={term.StartDate}
                        endDate={term.EndDate}
                        nextStart={term.NextStart}
                        key={"taskHistoryItem-" + index}
                        closeOnClick={true}>
                    </TaskHistoryItemRow>
                );
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <div>
                <div className="historyIcon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>
                <div className="taskHistoryList-title">{props.title}</div>
                <div className="taskHistoryList-grid">
                    {this.renderedHistoryListHeader()}
                    {this.renderedHistoryList()}
                </div>
                {this.renderPager()}
            </div>
        );
    }
}

HistoryPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    taskHistoryList: PropTypes.array,
    scheduleId: PropTypes.number,
    title: PropTypes.string,
    totalCount: PropTypes.number,
    pageSize: PropTypes.number
};

function mapStateToProps(state) {
    return {
        taskHistoryList: state.task.taskHistoryList,
        tabIndex: state.pagination.tabIndex,
        totalCount: state.task.totalHistoryCount
    };
}

export default connect(mapStateToProps)(HistoryPanelBody);