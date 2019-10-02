import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import "./style.less";
import { Switch, GridSystem as Grid, Button, Dropdown, SingleLineInputWithError  } from "@dnnsoftware/dnn-react-common";
import {
    logSettings as LogSettingActions
} from "../../../actions";
import util from "../../../utils";
import Localization from "localization";

/*eslint-disable eqeqeq*/
class LogSettingEditor extends Component {
    constructor() {
        super();
        this.state = {
            logSettingDetail: {
                KeepMostRecent: "*",
                LogTypeKey: "*",
                LogTypePortalID: "-1",
                LoggingIsActive: false,
                EmailNotificationIsActive: false,
                MailFromAddress: "",
                MailToAddress: "",
                NotificationThreshold: 1,
                NotificationThresholdTime: 1,
                NotificationThresholdTimeType: 1
            },
            triedToSubmit: false,
            formModified: false,
            error: {
                mailToAddress: false
            }
        };
    }

    componentWillMount() {
        const {props} = this;
        if (props.logTypeSettingId !== "") {
            props.dispatch(LogSettingActions.getLogSettingById({
                logTypeConfigId: props.logTypeSettingId
            }, (data) => {
                let logSettingDetail = Object.assign({}, data);
                this.setState({
                    logSettingDetail
                });
                this.SetErrorState();
            }));
        }
    }
    getValue(selectKey) {
        const {state} = this;
        switch (selectKey) {
            case "LogType":
                return state.logSettingDetail.LogTypeKey !== undefined ? state.logSettingDetail.LogTypeKey.toString() : "*";
            case "Website":
                return state.logSettingDetail.LogTypePortalID !== "-1" ? (state.logSettingDetail.LogTypePortalID.toString() == "*" ? "-1" : state.logSettingDetail.LogTypePortalID.toString()) : this.props.portalList[0].value.toString();
            case "Recent":
                return state.logSettingDetail.KeepMostRecent !== undefined && state.logSettingDetail.KeepMostRecent > 0 ? state.logSettingDetail.KeepMostRecent.toString() : "*";
            case "Threshold":
                return state.logSettingDetail.NotificationThreshold !== undefined && state.logSettingDetail.NotificationThreshold > 0 ? state.logSettingDetail.NotificationThreshold.toString() : "1";
            case "ThresholdNotificationTime":
                return state.logSettingDetail.NotificationThresholdTime !== undefined && state.logSettingDetail.NotificationThresholdTime > 0 ? state.logSettingDetail.NotificationThresholdTime.toString() : "1";
            case "ThresholdNotificationTimeType":
                return state.logSettingDetail.NotificationThresholdTimeType !== undefined && state.logSettingDetail.NotificationThresholdTimeType > 0 ? state.logSettingDetail.NotificationThresholdTimeType.toString() : "1";
            case "MailFromAddress":
                return state.logSettingDetail.MailFromAddress !== undefined ? state.logSettingDetail.MailFromAddress.toString() : "";
            case "MailToAddress":
                return state.logSettingDetail.MailToAddress !== undefined ? state.logSettingDetail.MailToAddress.toString() : "";
            default:
                break;
        }
    }
    getEnabledStatus(key) {
        const {state} = this;
        switch (key) {
            case "EmailNotification":
                return state.logSettingDetail.EmailNotificationIsActive !== undefined ? state.logSettingDetail.EmailNotificationIsActive : false;
            case "Logging":
                return state.logSettingDetail.LoggingIsActive !== undefined ? state.logSettingDetail.LoggingIsActive : false;
            default:
                break;
        }
    }
    onDropDownChange(key, option) {
        this.ProcessChange(key, option.value);
    }
    onTextChange(key, event) {
        this.ProcessChange(key, event.target.value);
    }
    ProcessChange(key, value) {
        let {logSettingDetail} = this.state;
        logSettingDetail[key] = value;
        this.setState({
            logSettingDetail: logSettingDetail
        });
        this.SetErrorState();
        let {state} = this;
        state.formModified = true;
        this.setState({
            state
        });
    }
    SetErrorState() {
        let {logSettingDetail} = this.state;
        let {state} = this;
        if (logSettingDetail.EmailNotificationIsActive) {
            if (!this.validateEmail(logSettingDetail.MailToAddress)) {
                state.error["mailToAddress"] = true;
            }
            else {
                state.error["mailToAddress"] = false;
            }
        } else {
            state.error["mailToAddress"] = false;
        }
        state.triedToSubmit = false;
        this.setState({
            state
        });
    }
    validateEmail(value) {
        const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(value);
    }
    OnCheckboxChanged(key, status) {
        let {logSettingDetail} = this.state;
        logSettingDetail[key] = status;
        this.setState({
            logSettingDetail: logSettingDetail
        });
        this.SetErrorState();
        let {state} = this;
        state.formModified = true;
        this.setState({
            state
        });
    }
    addUpdateLogSetting(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.mailToAddress) {
            return;
        }
        if (state.formModified) {
            let {logSettingDetail} = this.state;
            if (props.logTypeSettingId !== "") {
                props.dispatch(LogSettingActions.updateLogSetting(logSettingDetail, () => {
                    util.utilities.notify(Localization.get("ConfigUpdated"));
                    props.Collapse(event);
                }, () => {
                    util.utilities.notify(Localization.get("ConfigUpdateError"));
                }));
            } else {
                props.dispatch(LogSettingActions.addLogSetting(logSettingDetail, () => {
                    util.utilities.notify(Localization.get("ConfigAdded"));
                    props.Collapse(event);
                }, () => {
                    util.utilities.notify(Localization.get("ConfigAddError"));
                }));
            }
        } else {
            props.Collapse(event);
        }
    }

    deleteLogSetting(event) {
        const {props} = this;
        if (props.logTypeSettingId !== "") {
            util.utilities.confirm(Localization.get("ConfigDeletedWarning"), Localization.get("yes"), Localization.get("no"), () => {
                props.dispatch(LogSettingActions.deleteLogSetting({ LogTypeConfigId: props.logTypeSettingId }, () => {
                    util.utilities.notify(Localization.get("ConfigDeleted"));
                    props.Collapse(event);
                }, () => {
                    util.utilities.notify(Localization.get("DeleteError"));
                })
                );
            }, () => {
                util.utilities.notify(Localization.get("ConfigDeleteCancelled"));
            });
        }
        else {
            util.utilities.notify(Localization.get("ConfigDeleteInconsistency"));
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const portalList = this.props.portalList !== undefined && this.props.portalList.map(log => {
            return {
                label: log.label,
                value: log.value.toString()
            };
        });
        const columnOne = <div className="editor-container left-column">
            <div className="title-row divider">
                {Localization.get("Settings") }
            </div>
            <div className="status-row divider">
                <div className="left" title={Localization.get("plIsActive.Help") }>
                    { Localization.get("plIsActive") }
                </div>
                <div className="right">
                    <Switch 
                        value={this.getEnabledStatus("Logging") } 
                        onChange={this.OnCheckboxChanged.bind(this, "LoggingIsActive") }
                        onText={Localization.get("SwitchOn")}
                        offText={Localization.get("SwitchOff")}
                    />
                </div>
            </div>
            <div className="editor-row divider" title={Localization.get("plLogTypeKey.Help") }>
                <label>{Localization.get("plLogTypeKey") } </label>
                <Dropdown enabled={this.getEnabledStatus("Logging") } options={this.props.logTypeList } value={this.getValue("LogType") } onSelect={this.onDropDownChange.bind(this, "LogTypeKey") }
                    style={{ width: 100 + "%", float: "left" }}/>
            </div>
            <div className="editor-row divider" title={Localization.get("plLogTypePortalID.Help") }>
                <label>{Localization.get("plLogTypePortalID") } </label>
                <Dropdown enabled={this.getEnabledStatus("Logging") } options={portalList } value={this.getValue("Website") } onSelect={this.onDropDownChange.bind(this, "LogTypePortalID") }
                    style={{ width: 100 + "%", float: "left" }}/>
            </div>

            <div className="editor-row divider" >
                <label>{Localization.get("plKeepMostRecent") } </label>
                <Dropdown enabled={this.getEnabledStatus("Logging") } options={this.props.keepMostRecentOptions }  value={this.getValue("Recent") } onSelect={this.onDropDownChange.bind(this, "KeepMostRecent") }
                    style={{ width: 100 + "%", float: "left" }}/>
            </div>
        </div>;

        const columnTwo = <div className="editor-container">
            <div className="title-row divider">
                { Localization.get("EmailSettings") }
            </div>
            <div className="status-row divider">
                <div className="left" title={Localization.get("plEmailNotificationStatus.Help") }>
                    {  Localization.get("plEmailNotificationStatus") }
                </div>
                <div className="right">
                    <Switch 
                        value={this.getEnabledStatus("EmailNotification") } 
                        onChange={this.OnCheckboxChanged.bind(this, "EmailNotificationIsActive") }
                        onText={Localization.get("SwitchOn")}
                        offText={Localization.get("SwitchOff")}
                    />
                </div>
            </div>
            <div className="editor-row divider">
                <label>{Localization.get("plThreshold") }</label>
                <Dropdown enabled={this.getEnabledStatus("EmailNotification") } options={this.props.thresholdsOptions} value={this.getValue("Threshold") } onSelect={this.onDropDownChange.bind(this, "NotificationThreshold") }
                    style={{ width: 40 + "%", float: "left" }}/>
                <div className="text-section">in</div>
                <Dropdown enabled={this.getEnabledStatus("EmailNotification") } options={this.props.notificationTimesOptions } value={this.getValue("ThresholdNotificationTime") } onSelect={this.onDropDownChange.bind(this, "NotificationThresholdTime") }
                    style={{ width: 25 + "%", float: "left" }}/>
                <div className="text-section">&nbsp; </div>
                <Dropdown enabled={this.getEnabledStatus("EmailNotification") } options={this.props.notificationTimeTypesOptions } value={this.getValue("ThresholdNotificationTimeType") } onSelect={this.onDropDownChange.bind(this, "NotificationThresholdTimeType") }
                    style={{ width: 25 + "%", float: "left" }}/>
            </div>
            <div className="editor-row divider"  title={Localization.get("plMailToAddress.Help") }>
                <label>{Localization.get("plMailToAddress") } *</label>
                <SingleLineInputWithError
                    error={this.state.error.mailToAddress && this.state.triedToSubmit}
                    errorMessage={Localization.get("MailToAddress.Message") }
                    enabled={this.getEnabledStatus("EmailNotification") } value={this.getValue("MailToAddress") }
                    onChange={this.onTextChange.bind(this, "MailToAddress") }/>
            </div>
        </div>;
        let children = [];
        children.push(columnOne);
        children.push(columnTwo);
        /* eslint-disable react/no-danger */
        return (
            <div className="log-setting-editor">
                <Grid numberOfColumns={2}>{children}</Grid>
                <div className="buttons-box">
                    {this.props.logTypeSettingId !== "" && <Button type="secondary" onClick={this.deleteLogSetting.bind(this) }>{Localization.get("ConfigBtnDelete") }</Button>}
                    <Button type="secondary" onClick={this.props.Collapse.bind(this) }>{Localization.get("ConfigBtnCancel") }</Button>
                    <Button type="primary" onClick={this.addUpdateLogSetting.bind(this) }>{Localization.get("ConfigBtnSave") }</Button>
                </div>
            </div>
        );
    }
}

LogSettingEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    logTypeList: PropTypes.array.isRequired,
    logSettingDetail: PropTypes.object,
    portalList: PropTypes.array,
    logTypeSettingId: PropTypes.string,
    Collapse: PropTypes.func,
    keepMostRecentOptions: PropTypes.array.isRequired,
    thresholdsOptions: PropTypes.array.isRequired,
    notificationTimesOptions: PropTypes.array.isRequired,
    notificationTimeTypesOptions: PropTypes.array.isRequired
};

function mapStateToProps(state) {
    return {
        logSettingDetail: state.logSettings.logSettingDetail
    };
}

export default connect(mapStateToProps)(LogSettingEditor);