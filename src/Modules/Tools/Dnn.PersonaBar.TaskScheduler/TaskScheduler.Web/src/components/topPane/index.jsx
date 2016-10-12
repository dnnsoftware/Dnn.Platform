import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Collapsible from "react-collapse";
import {
    task as TaskActions
} from "../../actions";
import styles from "./style.less";
import Button from "dnn-button";
import ModePanel from "./modePanel";
import util from "../../utils";
import resx from "../../resources";

const svgIcon = require(`!raw!./../svg/edit.svg`);

class TopPane extends Component {
    constructor() {
        super();
        this.state = {
            modePanelOpen: false
        };
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(TaskActions.getSchedulerSettings());
    }

    onEnter(key, value) {
        const {props} = this;
        props.onEnter(key, value, props.index);
    }

    onClick() {
        const {props} = this;
        if (this.isStopped()) {
            props.dispatch(TaskActions.startSchedule(() => {
                util.utilities.notify(resx.get("SchedulerStartSuccess"));
                props.dispatch(TaskActions.getTaskStatusList());
            }, (error) => {
                util.utilities.notify(resx.get("SchedulerStartError"));
            }));
        }
        else {
            props.dispatch(TaskActions.stopSchedule(() => {
                util.utilities.notify(resx.get("SchedulerStopSuccess"));
                props.dispatch(TaskActions.getTaskStatusList());
            }, (error) => {
                util.utilities.notify(resx.get("SchedulerStopError"));
            }));
        }
    }

    getButtonDisplay() {
        if (this.isStopped() || this.props.schedulerMode === "2") {
            return resx.get("StartSchedule");
        }
        else {
            return resx.get("StopSchedule");
        }
    }

    isStopped() {
        const {props} = this;
        if (props.status === "STOPPED") {
            return true;
        }
        else {
            return false;
        }
    }

    getschedulerModeDisplay() {
        const {props} = this;
        if (props.schedulerMode) {
            return props.schedulerModeOptions[parseInt(props.schedulerMode)].Key;
        }
        return "";
    }

    toggleModePanel() {
        this.setState({
            modePanelOpen: !this.state.modePanelOpen
        });
    }

    getModeOptions() {
        const {props} = this;
        if (props.schedulerModeOptions) {
            let options = props.schedulerModeOptions.map((mode) => {
                return { value: mode.Value, label: mode.Key };
            });
            return options;
        }
        else return [{ value: "", label: "" }];
    }

    renderButton() {
        const {props} = this;
        if (props.schedulingEnabled === "True") {
            if (props.schedulerMode === "2") {
                return (<Button
                    type="primary" disabled={true}>
                    {this.getButtonDisplay() }
                </Button>);
            }
            else {
                return (<Button
                    className={props.status !== "STOPPED" ? "topPane-button-start" : ""}
                    type={props.status !== "STOPPED" ? "secondary" : "primary" }
                    onClick={this.onClick.bind(this) }>
                    {this.getButtonDisplay() }
                </Button>);
            }
        }
        else {
            return (<Button
                type="primary" disabled={true}>
                {this.getButtonDisplay() }
            </Button>);
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.topPane}>
                <div className="topPane-left">{resx.get("lblStatusLabel") }</div>
                <div className="topPane-middle">
                    <div className={props.status === "STOPPED" ? "topPane-middle-name-stopped" : "topPane-middle-name" }>{resx.get(props.status) }</div>
                    <div className="topPane-middle-common">
                        <div className="topPane-middle-common-title">{resx.get("plSchedulerMode") }</div>
                        <div>
                            <div className="editIcon" dangerouslySetInnerHTML={{ __html: svgIcon }} onClick={this.toggleModePanel.bind(this) }/>
                            <div className="collapsible-content">
                            {props.schedulerDelay &&
                                <ModePanel
                                    fixedHeight={200}
                                    isOpened={state.modePanelOpen}
                                    onClose={this.toggleModePanel.bind(this) }
                                    schedulerDelay={props.schedulerDelay}
                                    schedulerModeOptions={this.getModeOptions() }
                                    schedulerMode={props.schedulerMode}>
                                </ModePanel>
                            }
                            </div>
                        </div>
                        <div className="topPane-middle-common-value">{this.getschedulerModeDisplay() }</div>
                    </div>

                    <div className="topPane-middle-common">
                        <div className="topPane-middle-common-title">{resx.get("lblStartDelay") }</div>
                        <div className="topPane-middle-common-value">{props.schedulerDelay}</div>
                    </div>
                </div>
                <div className="topPane-right">
                    <div className="topPane-right-common">
                        <div className="topPane-right-common-title">{resx.get("lblMaxThreadsLabel") }</div>
                        <div className="topPane-right-common-value">{props.maxThreads}</div>
                    </div>
                    <div className="topPane-right-common">
                        <div className="topPane-right-common-title">{resx.get("lblActiveThreadsLabel") }</div>
                        <div className="topPane-right-common-value">{props.activeThreads}</div>
                    </div>
                    <div className="topPane-right-common">
                        <div className="topPane-right-common-title">{resx.get("lblFreeThreadsLabel") }</div>
                        <div className="topPane-right-common-value">{props.freeThreads}</div>
                    </div>
                </div>
                <div className="topPane-button">
                    { this.renderButton() }
                </div>
            </div>
        );
    }
}

TopPane.propTypes = {
    dispatch: PropTypes.func.isRequired,
    status: PropTypes.string,
    maxThreads: PropTypes.string,
    activeThreads: PropTypes.string,
    freeThreads: PropTypes.string,
    schedulerMode: PropTypes.string,
    schedulerDelay: PropTypes.number,
    schedulerModeOptions: PropTypes.array,
    schedulingEnabled: PropTypes.string
};

function mapStateToProps(state) {
    return {
        schedulingEnabled: state.task.schedulingEnabled,
        status: state.task.status,
        freeThreads: state.task.freeThreads,
        activeThreads: state.task.activeThreads,
        maxThreads: state.task.maxThreads,
        schedulerMode: state.task.schedulerMode,
        schedulerDelay: state.task.schedulerDelay,
        schedulerModeOptions: state.task.schedulerModeOptions
    };
}


export default connect(mapStateToProps)(TopPane);