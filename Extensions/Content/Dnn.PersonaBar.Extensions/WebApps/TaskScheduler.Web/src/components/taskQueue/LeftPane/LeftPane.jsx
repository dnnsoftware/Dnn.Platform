import React, {Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.less";
import resx from "../../../resources";

/*eslint-disable quotes*/
const svgIcon = require(`!raw-loader!./../taskStatusItemRow/svg/clock_stop.svg`).default;
const svgIcon2 = require(`!raw-loader!./../taskStatusItemRow/svg/cycle.svg`).default;

class LeftPane extends Component {
    constructor() {
        super();
    }
    onEnter(key, value) {
        const {props} = this;
        props.onEnter(key, value, props.index);
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <div className={styles.taskStatusItemLeftPane}>
                <div hidden={!this.props.nextStart} className="taskIcon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>
                <div hidden={!this.props.startDate} className="taskIconProcessing" dangerouslySetInnerHTML={{ __html: svgIcon2 }}></div>
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
    startDate: PropTypes.string
};

export default LeftPane;