import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import Button from "dnn-button";
import Switch from "dnn-switch";
import Dropdown from "dnn-dropdown";
import DatePicker from "dnn-date-picker";
import History from "../../history";
import {
    task as TaskActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";

const re = /^[1-9][0-9]?[0-9]?[0-9]?[0-9]?[0-9]?$/;
let retainHistoryNumOptions = [];
let timeLapseMeasurementOptions = [];
let attachToEventOptions = [];
let catchUpEnabledOptions = [];

class SchedulerEditor extends Component {
    constructor() {
        super();

        this.state = {
            scheduleItemDetail: undefined,
            error: {
                name: true,
                frequency: true,
                retry: false
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
        const { props } = this;
        /*if (props.scheduleItemDetail) {
            this.setState({
                scheduleItemDetail: props.scheduleItemDetail
            });
            return;
        }*/
        if (props.scheduleId) {
            props.dispatch(TaskActions.getGetScheduleItem({
                scheduleId: props.scheduleId
            }));
        }
        else {
            this.setState({
                scheduleItemDetail: {}
            });
        }

        retainHistoryNumOptions = [];
        retainHistoryNumOptions.push({ "value": "0", "label": resx.get("None") });
        retainHistoryNumOptions.push({ "value": "1", "label": "1" });
        retainHistoryNumOptions.push({ "value": "5", "label": "5" });
        retainHistoryNumOptions.push({ "value": "10", "label": "10" });
        retainHistoryNumOptions.push({ "value": "25", "label": "25" });
        retainHistoryNumOptions.push({ "value": "50", "label": "50" });
        retainHistoryNumOptions.push({ "value": "60", "label": "60" });
        retainHistoryNumOptions.push({ "value": "100", "label": "100" });
        retainHistoryNumOptions.push({ "value": "250", "label": "250" });
        retainHistoryNumOptions.push({ "value": "500", "label": "500" });
        retainHistoryNumOptions.push({ "value": "-1", "label": resx.get("All") });

        if (timeLapseMeasurementOptions.length === 0) {
            timeLapseMeasurementOptions.push({ "value": "s", "label": resx.get("Seconds") });
            timeLapseMeasurementOptions.push({ "value": "m", "label": resx.get("Minutes") });
            timeLapseMeasurementOptions.push({ "value": "h", "label": resx.get("Hours") });
            timeLapseMeasurementOptions.push({ "value": "d", "label": resx.get("Days") });
            timeLapseMeasurementOptions.push({ "value": "w", "label": resx.get("Weeks") });
            timeLapseMeasurementOptions.push({ "value": "mo", "label": resx.get("Months") });
            timeLapseMeasurementOptions.push({ "value": "y", "label": resx.get("Years") });
        }

        if (attachToEventOptions.length === 0) {
            attachToEventOptions.push({ "value": "", "label": resx.get("None") });
            attachToEventOptions.push({ "value": "APPLICATION_START", "label": "APPLICATION_START" });
        }

        if (catchUpEnabledOptions.length === 0) {
            catchUpEnabledOptions.push({ "value": "false", "label": resx.get("Disabled") });
            catchUpEnabledOptions.push({ "value": "true", "label": resx.get("Enabled.Label") });
        }
    }

    componentDidUpdate(props) {
        let { state } = this;
        if (props.scheduleItemDetail["TypeFullName"] === "" || props.scheduleItemDetail["TypeFullName"] === undefined) {
            state.error["name"] = true;
        }
        else if (props.scheduleItemDetail["TypeFullName"] !== "" && props.scheduleItemDetail["TypeFullName"] !== undefined) {
            state.error["name"] = false;
        }
        if (props.scheduleItemDetail["TimeLapse"] === "" || !re.test(props.scheduleItemDetail["TimeLapse"])) {
            state.error["frequency"] = true;
        }
        else if (props.scheduleItemDetail["TimeLapse"] !== "" && re.test(props.scheduleItemDetail["TimeLapse"])) {
            state.error["frequency"] = false;
        }
        if (props.scheduleItemDetail["RetryTimeLapse"] === -1 || props.scheduleItemDetail["RetryTimeLapse"] === undefined || props.scheduleItemDetail["RetryTimeLapse"] === "") {
            state.error["retry"] = false;
        }
        else {
            if (!re.test(props.scheduleItemDetail["RetryTimeLapse"])) {
                state.error["retry"] = true;
            }
            else {
                state.error["retry"] = false;
            }
        }

        this.setState({
            scheduleItemDetail: Object.assign({}, props.scheduleItemDetail),
            triedToSubmit: false,
            error: state.error
        });
    }

    runSchedule() {
        const { props } = this;
        let { scheduleItemDetail } = this.state;
        props.dispatch(TaskActions.runScheduleItem(scheduleItemDetail, () => {
            util.utilities.notify(resx.get("RunNow"));
        }, () => {
            util.utilities.notify(resx.get("RunNowError"));
        }));
    }

    isEmptyDate(date) {
        return !date || new Date(date).getFullYear() < 1970;
    }

    getValue(selectKey) {
        const { state } = this;
        switch (selectKey) {
            case "FriendlyName":
                return state.scheduleItemDetail.FriendlyName !== undefined ? state.scheduleItemDetail.FriendlyName.toString() : "";
            case "TypeFullName":
                return state.scheduleItemDetail.TypeFullName !== undefined ? state.scheduleItemDetail.TypeFullName.toString() : "";
            case "RetainHistoryNum":
                return state.scheduleItemDetail.RetainHistoryNum !== undefined ? state.scheduleItemDetail.RetainHistoryNum.toString() : "0";
            case "Servers":
                return state.scheduleItemDetail.Servers !== undefined ? state.scheduleItemDetail.Servers.toString() : "";
            case "ObjectDependencies":
                return state.scheduleItemDetail.ObjectDependencies !== undefined ? state.scheduleItemDetail.ObjectDependencies.toString() : "";
            case "ScheduleStartDate":
                if (!this.isEmptyDate(state.scheduleItemDetail.ScheduleStartDate)) {
                    return new Date(state.scheduleItemDetail.ScheduleStartDate);
                }
                else {
                    return null;
                }
            case "TimeLapse":
                return state.scheduleItemDetail.TimeLapse !== undefined ? state.scheduleItemDetail.TimeLapse.toString() : "";
            case "TimeLapseMeasurement":
                return state.scheduleItemDetail.TimeLapseMeasurement !== undefined ? state.scheduleItemDetail.TimeLapseMeasurement.toString() : "s";
            case "RetryTimeLapse":
                return state.scheduleItemDetail.RetryTimeLapse !== undefined ? state.scheduleItemDetail.RetryTimeLapse.toString() : "";
            case "RetryTimeLapseMeasurement":
                return state.scheduleItemDetail.RetryTimeLapseMeasurement !== undefined ? state.scheduleItemDetail.RetryTimeLapseMeasurement.toString() : "s";
            case "AttachToEvent":
                return state.scheduleItemDetail.AttachToEvent !== undefined ? state.scheduleItemDetail.AttachToEvent.toString() : "";
            case "CatchUpEnabled":
                return state.scheduleItemDetail.CatchUpEnabled !== undefined ? state.scheduleItemDetail.CatchUpEnabled.toString() : "false";
            case "Enabled":
                return state.scheduleItemDetail.Enabled !== undefined ? state.scheduleItemDetail.Enabled.toString() : "false";
            default:
                break;
        }
    }

    onSettingChange(key, event) {
        let { state, props } = this;
        let scheduleItemDetail = Object.assign({}, state.scheduleItemDetail);

        if (key === "ScheduleStartDate") {
            scheduleItemDetail[key] = event;
        }
        else if (key === "CatchUpEnabled") {
            scheduleItemDetail[key] = event.value === "true" ? true : false;
        }
        else if (key === "RetainHistoryNum" || key === "TimeLapseMeasurement"
            || key === "RetryTimeLapseMeasurement" || key === "AttachToEvent") {
            scheduleItemDetail[key] = event.value;
        }
        else {
            scheduleItemDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }
        if (scheduleItemDetail[key] === "" && key === "TypeFullName") {
            state.error["name"] = true;
        }
        else if (scheduleItemDetail[key] !== "" && key === "TypeFullName") {
            state.error["name"] = false;
        }
        if ((scheduleItemDetail[key] === "" || !re.test(scheduleItemDetail[key])) && key === "TimeLapse") {
            state.error["frequency"] = true;
        }
        else if (scheduleItemDetail[key] !== "" && re.test(scheduleItemDetail[key]) && key === "TimeLapse") {
            state.error["frequency"] = false;
        }
        if (scheduleItemDetail[key] === "" && key === "RetryTimeLapse") {
            state.error["retry"] = false;
        }
        else if (key === "RetryTimeLapse") {
            if (!re.test(scheduleItemDetail[key])) {
                state.error["retry"] = true;
            }
            else {
                state.error["retry"] = false;
            }
        }
        this.setState({
            scheduleItemDetail: scheduleItemDetail,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(TaskActions.settingsClientModified(scheduleItemDetail));
    }

    onUpdateItem(event) {
        event.preventDefault();
        const { props, state } = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name || state.error.frequency || state.error.retry) {
            return;
        }

        props.onUpdate(this.state.scheduleItemDetail);
    }

    onCancel() {
        const { props } = this;
        if (props.settingsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(TaskActions.cancelSettingsClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        if (this.props.panelIndex === 1) {
            return <History pageSize={5} scheduleId={this.props.scheduleId} title={resx.get("HistoryModalTitle")} />;
        }

        if (this.state.scheduleItemDetail !== undefined || this.props.id === "add") {
            const columnOne = <div className="container">
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plFriendlyName")}
                        error={false}
                        value={this.getValue("FriendlyName" || "")}
                        onChange={this.onSettingChange.bind(this, "FriendlyName")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plType") + " *"}
                        error={this.state.error.name && this.state.triedToSubmit}
                        errorMessage={resx.get("TypeRequired")}
                        value={this.getValue("TypeFullName") || ""}
                        onChange={this.onSettingChange.bind(this, "TypeFullName")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plRetainHistoryNum")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={retainHistoryNumOptions}
                        value={this.getValue("RetainHistoryNum")}
                        onSelect={this.onSettingChange.bind(this, "RetainHistoryNum")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("Servers")}
                        error={false}
                        value={this.getValue("Servers") || ""}
                        onChange={this.onSettingChange.bind(this, "Servers")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plObjectDependencies")}
                        error={false}
                        value={this.getValue("ObjectDependencies") || ""}
                        onChange={this.onSettingChange.bind(this, "ObjectDependencies")}
                    />
                </div>
            </div>;
            const columnTwo = <div className="container right-column">
                <div className="editor-row divider">
                    <Label label={resx.get("plScheduleStartDate")} style={{ margin: "0 0 5px 0" }} />
                    <DatePicker
                        date={this.getValue("ScheduleStartDate")}
                        updateDate={this.onSettingChange.bind(this, "ScheduleStartDate")}
                        isDateRange={false}
                        hasTimePicker={true}
                        showClearDateButton={true}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ float: "left", width: "47.5%", whiteSpace: "pre" }}
                        label={resx.get("plTimeLapse") + " *"}
                        error={this.state.error.frequency && this.state.triedToSubmit}
                        errorMessage={resx.get("TimeLapseRequired.ErrorMessage")}
                        value={this.getValue("TimeLapse") || ""}
                        onChange={this.onSettingChange.bind(this, "TimeLapse")}
                    />
                    <div className="text-section">&nbsp; </div>
                    <Dropdown
                        style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
                        options={timeLapseMeasurementOptions}
                        value={this.getValue("TimeLapseMeasurement")}
                        onSelect={this.onSettingChange.bind(this, "TimeLapseMeasurement")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: "47.5%", float: "left", whiteSpace: "pre" }}
                        label={resx.get("plRetryTimeLapse")}
                        error={this.state.error.retry && this.state.triedToSubmit}
                        errorMessage={resx.get("RetryTimeLapseValidator.ErrorMessage")}
                        value={this.getValue("RetryTimeLapse") === "-1" ? "" : this.getValue("RetryTimeLapse")}
                        onChange={this.onSettingChange.bind(this, "RetryTimeLapse")}
                    />
                    <div className="text-section">&nbsp; </div>
                    <Dropdown
                        style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
                        options={timeLapseMeasurementOptions}
                        value={this.getValue("RetryTimeLapseMeasurement")}
                        onSelect={this.onSettingChange.bind(this, "RetryTimeLapseMeasurement")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plAttachToEvent")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={attachToEventOptions}
                        value={this.getValue("AttachToEvent")}
                        onSelect={this.onSettingChange.bind(this, "AttachToEvent")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plCatchUpEnabled")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={catchUpEnabledOptions}
                        value={this.getValue("CatchUpEnabled")}
                        onSelect={this.onSettingChange.bind(this, "CatchUpEnabled")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plEnabled")} style={{ width: "47.5%" }} />
                    <div className="right">
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={this.getValue("Enabled") === "true" ? true : false}
                            onChange={this.onSettingChange.bind(this, "Enabled")}
                        />
                    </div>
                </div>
            </div>;

            return (
                <div className="scheduler-setting-editor">
                    <Grid numberOfColumns={2}>{[columnOne, columnTwo]}</Grid>
                    <div className="buttons-box">
                        {this.props.scheduleId !== undefined && <Button type="secondary" onClick={this.props.onDelete.bind(this, this.props.scheduleId)}>{resx.get("cmdDelete")}</Button>}
                        <Button
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        {this.props.scheduleId !== undefined && this.props.scheduleItemDetail.Enabled && this.props.enabled &&
                            <Button type="secondary" onClick={this.runSchedule.bind(this)}>{resx.get("cmdRun")}</Button>
                        }
                        <Button
                            disabled={!this.props.settingsClientModified}
                            type="primary"
                            onClick={this.onUpdateItem.bind(this)}>
                            {this.props.scheduleId !== undefined ? resx.get("Update") : resx.get("cmdSave")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SchedulerEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    scheduleItemDetail: PropTypes.object,
    enabled: PropTypes.bool,
    serverList: PropTypes.array,
    scheduleId: PropTypes.number,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    settingsClientModified: PropTypes.bool,
    panelIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        scheduleItemDetail: state.task.scheduleItemDetail,
        settingsClientModified: state.task.settingsClientModified
    };
}

export default connect(mapStateToProps)(SchedulerEditor);