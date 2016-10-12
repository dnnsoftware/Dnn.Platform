import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import LeftPane from "../LeftPane";
import RightPane from "../RightPane";
import {
    task as TaskActions
} from "../../../actions";

class TaskStatusItemRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.timeout = 0;
        // setInterval(() => {
        //     console.log("Repainting: ", this.state.repainting);
        // }, 500);
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

    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!ReactDOM.findDOMNode(this).contains(event.target) && (typeof event.target.className == "string" && event.target.className.indexOf("do-not-close") == -1)) {

            this.timeout = 475;
        } else {

            this.timeout = 0;
        }
    }

    render() {
        const {props, state} = this;
        return (
            <div className={styles.taskStatusItemRow}>
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
    remainingTime: PropTypes.number,
    nextStart: PropTypes.string,
    objectDependencies: PropTypes.string,
    scheduleSource: PropTypes.string,
    servers: PropTypes.string,
    threadId: PropTypes.number,
    elapsedTime: PropTypes.number,
    startDate: PropTypes.string,
    children: PropTypes.node
};

function mapStateToProps(state) {
    return {

    };
}

export default connect(mapStateToProps)(TaskStatusItemRow);
