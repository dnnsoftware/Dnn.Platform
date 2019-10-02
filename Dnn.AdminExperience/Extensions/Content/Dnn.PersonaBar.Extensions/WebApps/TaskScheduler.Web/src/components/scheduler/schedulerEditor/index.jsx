import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { 
    Label,
    Button,
    Switch,
    Dropdown,
    DatePicker,
    SingleLineInputWithError
} from "@dnnsoftware/dnn-react-common";
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
        if (props.scheduleId) {
            props.dispatch(TaskActions.getGetScheduleItem({
                scheduleId: props.scheduleId
            }));
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

        this.setState({
            triedToSubmit: false,
            error: {}
        });
    }

    runSchedule() {
        const { props } = this;
        let { scheduleItemDetail } = props;
        props.dispatch(TaskActions.runScheduleItem(scheduleItemDetail, () => {
            util.utilities.notify(resx.get("RunNow"));
        }, () => {
            util.utilities.notify(resx.get("RunNowError"));
        }));
    }

    isEmptyDate(date) {
        return !date || new Date(date).getFullYear() < 1970;
    }

    getDefaultIfNull(obj, key, defaultValue) {
        if (obj && key && obj[key]) {
            return obj[key].toString();
        }
        return defaultValue;
    }

    getValue(selectKey) {
        const { props } = this;
        switch (selectKey) {
            case "FriendlyName":
                return this.getDefaultIfNull(props.scheduleItemDetail, "FriendlyName", "");
            case "TypeFullName":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TypeFullName", "");
            case "RetainHistoryNum":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetainHistoryNum", "0");
            case "Servers":
                return this.getDefaultIfNull(props.scheduleItemDetail, "Servers", "");
            case "ObjectDependencies":
                return this.getDefaultIfNull(props.scheduleItemDetail, "ObjectDependencies", "");
            case "ScheduleStartDate":
                if (!this.isEmptyDate(this.getDefaultIfNull(props.scheduleItemDetail, "ScheduleStartDate", null))) {
                    return new Date(props.scheduleItemDetail.ScheduleStartDate);
                }
                else {
                    return null;
                }
            case "TimeLapse":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TimeLapse", "");
            case "TimeLapseMeasurement":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TimeLapseMeasurement", "s");
            case "RetryTimeLapse":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetryTimeLapse", "");
            case "RetryTimeLapseMeasurement":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetryTimeLapseMeasurement", "s");
            case "AttachToEvent":
                return this.getDefaultIfNull(props.scheduleItemDetail, "AttachToEvent", "");
            case "CatchUpEnabled":
                return this.getDefaultIfNull(props.scheduleItemDetail, "CatchUpEnabled", "false");
            case "Enabled":
                return this.getDefaultIfNull(props.scheduleItemDetail, "Enabled", "false");
            default:
                break;
        }
    }

    onSettingChange(key, event) {
        let { state, props } = this;
        let scheduleItemDetail = Object.assign({}, props.scheduleItemDetail);

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

        props.onUpdate(props.scheduleItemDetail);
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
        const { props, state } = this;
        if (props.panelIndex === 1) {
            return <History pageSize={5} scheduleId={props.scheduleId} title={resx.get("HistoryModalTitle")} />;
        }

        if (props.scheduleItemDetail !== undefined || props.id === "add") {
            const columnOne = <div className="container" key="columnOne">
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
                        error={state.error.name && state.triedToSubmit}
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
            const columnTwo = <div className="container right-column" key="columnTwo">
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
                        error={state.error.frequency && state.triedToSubmit}
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
                        error={state.error.retry && state.triedToSubmit}
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
                    <div className="scheduler-item-container">
                        <div className="scheduler-item-column">
                            {columnOne}
                        </div>
                        <div className="scheduler-item-column">
                            {columnTwo}
                        </div>
                    </div>
                    <div className="buttons-box">
                        {props.scheduleId !== undefined && <Button type="secondary" onClick={props.onDelete.bind(this, props.scheduleId)}>{resx.get("cmdDelete")}</Button>}
                        <Button
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        {props.scheduleId !== undefined && props.scheduleItemDetail.Enabled && props.enabled &&
                            <Button type="secondary" onClick={this.runSchedule.bind(this)}>{resx.get("cmdRun")}</Button>
                        }
                        <Button
                            disabled={!props.settingsClientModified}
                            type="primary"
                            onClick={this.onUpdateItem.bind(this)}>
                            {props.scheduleId !== undefined ? resx.get("Update") : resx.get("cmdSave")}
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
