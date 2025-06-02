import React, {Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
import resx from "../../../resources";
import clockStopIcon from "./../taskStatusItemRow/svg/clock_stop.svg?raw";
import cycleIcon from "./../taskStatusItemRow/svg/cycle.svg?raw";

class LeftPane extends Component {
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
            <div className={styles.taskStatusItemLeftPane}>
                <div hidden={!this.props.nextStart} className="taskIcon" dangerouslySetInnerHTML={{ __html: clockStopIcon }}></div>
                <div hidden={!this.props.startDate} className="taskIconProcessing" dangerouslySetInnerHTML={{ __html: cycleIcon }}></div>
                <div className="taskDetail">
                    <div className="taskDetail-name">{props.friendlyName}</div>
                    <div hidden={!this.props.startDate} className="taskDetail-common">
                        <div className="taskDetail-common-title-processing">{resx.get("processing")}</div>
                    </div>
                    <div hidden={!this.props.nextStart} className="taskDetail-common">
                        <div className="taskDetail-common-title">{resx.get("NextStart.Label")}</div>
                        <div className="taskDetail-common-value">{props.nextStart}</div>
                    </div>
                    <div hidden={!this.props.startDate} className="taskDetail-common">
                        <div className="taskDetail-common-title">{resx.get("Started.Header")}</div>
                        <div className="taskDetail-common-value">{props.startDate}</div>
                    </div>
                    <div hidden={!this.props.nextStart} className="taskDetail-common">
                        <div className="taskDetail-common-title">{resx.get("Overdue.Header")}</div>
                        <div className="taskDetail-common-value">{props.overdue}</div>
                    </div>
                    <div hidden={!this.props.startDate} className="taskDetail-common">
                        <div className="taskDetail-common-title">{resx.get("Duration.Header")}</div>
                        <div className="taskDetail-common-value">{props.elapsedTime}</div>
                    </div>
                    <div hidden={!this.props.nextStart} className="taskDetail-common">
                        <div className="taskDetail-common-title">{resx.get("TimeRemaining.Header")}</div>
                        <div className="taskDetail-common-value">{props.remainingTime}</div>
                    </div>
                </div>
            </div>
        );
    }
}

LeftPane.propTypes = {
    friendlyName: PropTypes.string,
    nextStart: PropTypes.string,
    overdue: PropTypes.bool,
    remainingTime: PropTypes.string,
    elapsedTime: PropTypes.number,
    startDate: PropTypes.string,
    onEnter: PropTypes.func,
    index: PropTypes.number
};

export default LeftPane;