import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { 
    InputGroup,
    Switch,
    Label,
    Button,
    SingleLineInputWithError,
    MultiLineInputWithError,
    Tooltip
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

const re = /^[1-9][0-9]?[0-9]?$|^0$/;
const re2 = /^([9]\d|\d{3,4})$/;
const re3 = /^([1][2-9]|[2-9][0-9]|\d{3,})$/;

class OtherSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            otherSettings: undefined,
            error: {
                autoAccountUnlockDuration: false,
                asyncTimeout: false,
                maxUploadSize: false
            },
            triedToSubmit: false
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.otherSettings) {
            this.setState({
                otherSettings: props.otherSettings
            });
            return;
        }
        props.dispatch(SecurityActions.getOtherSettings((data) => {
            let otherSettings = Object.assign({}, data.Results.Settings);
            this.setState({
                otherSettings
            });
        }));
    }

    UNSAFE_componentWillReceiveProps(props) {
        let {state} = this;

        let autoAccountUnlockDuration = props.otherSettings["AutoAccountUnlockDuration"];
        if (autoAccountUnlockDuration === "" || !re.test(autoAccountUnlockDuration)) {
            state.error["autoAccountUnlockDuration"] = true;
        }
        else if (autoAccountUnlockDuration !== "" && re.test(autoAccountUnlockDuration)) {
            state.error["autoAccountUnlockDuration"] = false;
        }
        let asyncTimeout = props.otherSettings["AsyncTimeout"];
        if (asyncTimeout === "" || !re2.test(asyncTimeout)) {
            state.error["asyncTimeout"] = true;
        }
        else if (asyncTimeout !== "" && re2.test(asyncTimeout)) {
            state.error["asyncTimeout"] = false;
        }
        let maxUploadSize = props.otherSettings["MaxUploadSize"];
        if (!this.isMaxUploadSizeValid(maxUploadSize)) {
            state.error["maxUploadSize"] = true;
        }
        else if (this.isMaxUploadSizeValid(maxUploadSize)) {
            state.error["maxUploadSize"] = false;
        }

        this.setState({
            otherSettings: Object.assign({}, props.otherSettings),
            triedToSubmit: false,
            error: state.error
        });
    }

    isMaxUploadSizeValid(size) {
        let {props} = this;
        if (props.otherSettings === undefined) {
            return true;
        }
        let rangeUploadSize = parseInt(props.otherSettings["RangeUploadSize"]);
        if (size === "") {
            return false;
        }
        else {
            let maxUploadSize = parseInt(size);
            if (!re3.test(maxUploadSize) || maxUploadSize > rangeUploadSize) {
                return false;
            }
            else {
                return true;
            }
        }
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let otherSettings = Object.assign({}, state.otherSettings);
        otherSettings[key] = typeof (event) === "object" ? event.target.value : event;

        if (!re.test(otherSettings[key]) && key === "AutoAccountUnlockDuration") {
            state.error["autoAccountUnlockDuration"] = true;
        }
        else if (re.test(otherSettings[key]) && key === "AutoAccountUnlockDuration") {
            state.error["autoAccountUnlockDuration"] = false;
        }
        if (!re2.test(otherSettings[key]) && key === "AsyncTimeout") {
            state.error["asyncTimeout"] = true;
        }
        else if (re2.test(otherSettings[key]) && key === "AsyncTimeout") {
            state.error["asyncTimeout"] = false;
        }
        if (key === "MaxUploadSize" && !this.isMaxUploadSizeValid(otherSettings[key])) {
            state.error["maxUploadSize"] = true;
        }
        else if (key === "MaxUploadSize" && this.isMaxUploadSizeValid(otherSettings[key])) {
            state.error["maxUploadSize"] = false;
        }

        this.setState({
            otherSettings: otherSettings,
            error: state.error,
            triedToSubmit: false
        });
        props.dispatch(SecurityActions.otherSettingsClientModified(otherSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.autoAccountUnlockDuration || state.error.asyncTimeout || state.error.maxUploadSize) {
            return;
        }

        let parameters = Object.assign({}, state.otherSettings);
        props.dispatch(SecurityActions.updateOtherSettings(parameters, () => {
            util.utilities.notify(resx.get("OtherSettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("OtherSettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("OtherSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getOtherSettings((data) => {
                let otherSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    otherSettings
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const { state } = this;
        if (state.otherSettings) {
            return (
                <div className={styles.otherSettings}>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plDisplayCopyright.Help")}
                                label={resx.get("plDisplayCopyright")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.DisplayCopyright}
                                onChange={this.onSettingChange.bind(this, "DisplayCopyright")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plShowCriticalErrors.Help")}
                                label={resx.get("plShowCriticalErrors")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.ShowCriticalErrors}
                                onChange={this.onSettingChange.bind(this, "ShowCriticalErrors")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plDebugMode.Help")}
                                label={resx.get("plDebugMode")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.DebugMode}
                                onChange={this.onSettingChange.bind(this, "DebugMode")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plRememberMe.Help")}
                                label={resx.get("plRememberMe")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.RememberCheckbox}
                                onChange={this.onSettingChange.bind(this, "RememberCheckbox")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAutoAccountUnlock.Help")}
                            label={resx.get("plAutoAccountUnlock")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.autoAccountUnlockDuration && this.state.triedToSubmit}
                            errorMessage={resx.get("AutoAccountUnlockDuration.ErrorMessage")}
                            value={state.otherSettings.AutoAccountUnlockDuration}
                            onChange={this.onSettingChange.bind(this, "AutoAccountUnlockDuration")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAsyncTimeout.Help")}
                            label={resx.get("plAsyncTimeout")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.asyncTimeout && this.state.triedToSubmit}
                            errorMessage={resx.get("AsyncTimeout.ErrorMessage")}
                            value={state.otherSettings.AsyncTimeout}
                            onChange={this.onSettingChange.bind(this, "AsyncTimeout")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plMaxUploadSize.Help")}
                            label={resx.get("plMaxUploadSize")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.maxUploadSize && this.state.triedToSubmit}
                            errorMessage={resx.get("maxUploadSize.Error").replace("{0}", state.otherSettings.RangeUploadSize)}
                            value={state.otherSettings.MaxUploadSize}
                            onChange={this.onSettingChange.bind(this, "MaxUploadSize")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plFileExtensions.Help")}
                            label={resx.get("plFileExtensions")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <MultiLineInputWithError
                            value={state.otherSettings.AllowedExtensionWhitelist}
                            onChange={this.onSettingChange.bind(this, "AllowedExtensionWhitelist")} />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.otherSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.otherSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

OtherSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    otherSettings: PropTypes.object,
    otherSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        otherSettings: state.security.otherSettings,
        otherSettingsClientModified: state.security.otherSettingsClientModified
    };
}

export default connect(mapStateToProps)(OtherSettingsPanelBody);