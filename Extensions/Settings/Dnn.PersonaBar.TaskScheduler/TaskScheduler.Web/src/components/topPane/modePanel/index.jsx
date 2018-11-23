import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Collapse from "dnn-collapsible";
import Button from "dnn-button";
import Select from "dnn-select";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import "./style.less";
import {
    task as TaskActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";

const re = /^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-4][0-4][0])$/;

class ModePanel extends Component {
    constructor() {
        super();
        this.state = {
            updateRequest: {
                SchedulerMode: "",
                SchedulerdelayAtAppStart: ""
            },
            error: {
                schedulerDelay: false
            },
            triedToSubmit: false,
            clientModified: false
        };
    }

    componentDidMount() {
        let {props, state} = this;
        if (props.schedulerDelay === "" || !re.test(props.schedulerDelay)) {
            state.error["schedulerDelay"] = true;
        }
        this.setState({
            triedToSubmit: false,
            error: state.error,
            updateRequest: {
                SchedulerMode: props.schedulerMode,
                SchedulerdelayAtAppStart: props.schedulerDelay
            }
        });        
    }    

    onSave(event) {
        event.preventDefault();
        const {props, state} = this;

        this.setState({
            triedToSubmit: true
        });

        if (state.error.schedulerDelay) {
            return;
        }

        props.dispatch(TaskActions.updateSchedulerSettings(state.updateRequest, () => {
            util.utilities.notify(resx.get("SchedulerUpdateSuccess"));
        }, () => {
            util.utilities.notify(resx.get("SchedulerUpdateError"));
        }));

        this.setState({
            clientModified: false
        });

        props.onClose();
    }

    onValueChange(key, event) {
        let {state} = this;
        const value = event.target.value;
        const { updateRequest } = this.state;
        updateRequest[key] = value;

        if (!re.test(updateRequest[key]) && key === "SchedulerdelayAtAppStart") {
            state.error["schedulerDelay"] = true;
        }
        else if (re.test(updateRequest[key]) && key === "SchedulerdelayAtAppStart") {
            state.error["schedulerDelay"] = false;
        }

        this.setState({
            triedToSubmit: false,
            updateRequest,
            error: state.error,
            clientModified: true
        });
    }

    onClose() {
        const {props, state} = this;
        state.error["schedulerDelay"] = false;
        this.setState({
            error: state.error,
            clientModified: false
        });
        props.onClose();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={props.isOpened ? "collapsible-content-mode" : "collapsible-content-mode-hidden"}>
                <Collapse
                    fixedHeight={props.fixedHeight}
                    keepCollapsedContent={props.keepCollapsedContent}
                    isOpened={props.isOpened}>
                    {props.fixedHeight &&
                        <div>
                            <div className="modepanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                                <div className="">
                                    <div className="editor-row divider">
                                        <label>{resx.get("plSchedulerMode")}</label>
                                        <Select
                                            onChange={this.onValueChange.bind(this, "SchedulerMode")}
                                            options={props.schedulerModeOptions}
                                            value={state.updateRequest.SchedulerMode} />
                                    </div>
                                    <div className="editor-row divider">
                                        <label>{resx.get("plScheduleAppStartDelay")}</label>
                                        <SingleLineInputWithError
                                            inputStyle={{ margin: "0" }}
                                            withLabel={false}
                                            error={this.state.error.schedulerDelay && this.state.triedToSubmit}
                                            errorMessage={resx.get("ScheduleAppStartDelayValidation")}
                                            value={state.updateRequest.SchedulerdelayAtAppStart}
                                            onChange={this.onValueChange.bind(this, "SchedulerdelayAtAppStart")}
                                        />
                                    </div>
                                    <div className="action-buttons">
                                        <Button type="secondary" onClick={this.onClose.bind(this)}>{resx.get("Cancel")}</Button>
                                        <Button
                                            type="primary"
                                            disabled={!state.clientModified}
                                            onClick={this.onSave.bind(this)}>{resx.get("Update")}
                                        </Button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                    {!props.fixedHeight && props.children}
                </Collapse>
            </div>
        );
    }
}

ModePanel.PropTypes = {
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired,
    schedulerMode: PropTypes.string.isRequired,
    schedulerDelay: PropTypes.number.isRequired,
    schedulerModeOptions: PropTypes.array.isRequired,
    clientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        schedulerDelay: state.task.schedulerDelay,
        schedulerMode: state.task.schedulerMode,
        clientModified: state.task.settingsClientModified
    };
}

export default connect(mapStateToProps)(ModePanel);