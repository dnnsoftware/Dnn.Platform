import React, {Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.less";
import resx from "../../../resources";

class RightPane extends Component {
    constructor() {
        super();
    }
    onEnter(key, value) {
        const {props} = this;
        props.onEnter(key, value, props.index);
    }
    render() {
        const {props} = this;
        return (
            <div className={styles.taskStatusItemRightPane}>
                <div className="taskDetailRight">
                    <div className="taskDetailRight-idWrapper">
                        <div className="taskDetailRight-id">{resx.get("ScheduleID.Header")}</div>
                        <div className="taskDetailRight-id-value">{props.scheduleId}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("Servers.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.servers}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("ObjectDependencies.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.objectDependencies}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("TriggeredBy.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.scheduleSource}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("Thread.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.threadId}</div>
                    </div>
                </div>
            </div>
        );
    }
}

RightPane.propTypes = {
    scheduleId: PropTypes.number,
    servers: PropTypes.string,
    objectDependencies: PropTypes.string,
    threadId: PropTypes.number,
    scheduleSource: PropTypes.string
};

export default RightPane;