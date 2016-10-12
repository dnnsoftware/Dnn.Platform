import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    task as TaskActions
} from "../../actions";
import TaskStatusItemRow from "./taskStatusItemRow";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

class TaskQueuePanelBody extends Component {
    constructor() {
        super();
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(TaskActions.getTaskStatusList());
    }


    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
    }

    /* eslint-disable react/no-danger */
    renderedTaskStatusList() {
        const {props} = this;
        return props.taskStatusList.map((term, index) => {
            return (
                <TaskStatusItemRow
                    scheduleId={term.ScheduleID}
                    friendlyName={term.FriendlyName}
                    overdue={term.Overdue}
                    remainingTime={term.RemainingTime}
                    nextStart={term.NextStart}
                    objectDependencies={term.ObjectDependencies}
                    scheduleSource={term.ScheduleSource}
                    threadId={term.ThreadID}
                    servers={term.Servers}
                    key={"taskStatusItem-" + index}
                    closeOnClick={true}>
                </TaskStatusItemRow>
            );
        });
    }

    renderedTaskProcessingList() {
        const {props} = this;
        if (props.taskProcessingList) {
            return props.taskProcessingList.map((term, index) => {
                return (
                    <TaskStatusItemRow
                        scheduleId={term.ScheduleID}
                        friendlyName={term.TypeFullName}
                        elapsedTime={term.ElapsedTime}
                        startDate={term.StartDate}
                        objectDependencies={term.ObjectDependencies}
                        scheduleSource={term.ScheduleSource}
                        threadId={term.ThreadID}
                        servers={term.Servers}
                        key={"taskStatusItem-" + index}
                        closeOnClick={true}>
                    </TaskStatusItemRow>
                );
            });
        }
    }

    render() {
        const {props, state} = this;
        return (
            <div>
                <div>
                    <div className={props.schedulingEnabled === "True" ? "taskStatusList-title" : "taskStatusList-disabled"}>
                        {props.schedulingEnabled === "True" ? resx.get("TaskQueueTitle") : resx.get("DisabledMessage") }
                    </div>
                    { this.renderedTaskProcessingList() }
                    { this.renderedTaskStatusList() }
                </div>
            </div>
        );
    }
}

TaskQueuePanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    schedulingEnabled: PropTypes.string,
    taskStatusList: PropTypes.array,
    taskProcessingList: PropTypes.array
};

function mapStateToProps(state) {
    return {
        schedulingEnabled: state.task.schedulingEnabled,
        taskStatusList: state.task.taskStatusList,
        taskProcessingList: state.task.taskProcessingList,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(TaskQueuePanelBody);