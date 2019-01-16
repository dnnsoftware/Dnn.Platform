import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.less";
import { GridSystem } from "@dnnsoftware/dnn-react-common";
import LeftPane from "../LeftPane";
import RightPane from "../RightPane";

class TaskStatusItemRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }

    /*eslint-disable eqeqeq*/
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!this.node.contains(event.target) && (typeof event.target.className == "string" && event.target.className.indexOf("do-not-close") == -1)) {

            this.timeout = 475;
        } else {

            this.timeout = 0;
        }
    }

    render() {
        const {props} = this;
        return (
            <div className={styles.taskStatusItemRow} ref={node => this.node = node}>
                <GridSystem>
                    <LeftPane
                        friendlyName={props.friendlyName}
                        nextStart={props.nextStart}
                        overdue={props.overdue}
                        remainingTime={props.remainingTime}
                        elapsedTime={props.elapsedTime}
                        startDate={props.startDate}
                        key={"schedule-left-" + props.scheduleId}
                    />
                    <RightPane
                        scheduleId={props.scheduleId}
                        servers={props.servers}
                        objectDependencies={props.objectDependencies}
                        threadId={props.threadId}
                        scheduleSource={props.scheduleSource}
                        key={"schedule-right-" + props.scheduleId}
                    />
                </GridSystem>
            </div>
        );
    }
}

TaskStatusItemRow.propTypes = {
    scheduleId: PropTypes.number,
    friendlyName: PropTypes.string,
    overdue: PropTypes.bool,
    remainingTime: PropTypes.string,
    nextStart: PropTypes.string,
    objectDependencies: PropTypes.string,
    scheduleSource: PropTypes.string,
    servers: PropTypes.string,
    threadId: PropTypes.number,
    elapsedTime: PropTypes.number,
    startDate: PropTypes.string,
    children: PropTypes.node
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(TaskStatusItemRow);
