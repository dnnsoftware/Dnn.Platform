import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
    Dropdown,
    InputGroup,
    Switch,
    Label,
    Button,
    Tooltip,
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import "./style.less";

let timespanSiteOptions = [];
let timespanUserOptions = [];

class ApiTokenSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            apiTokenSettings: undefined,
        };
        timespanSiteOptions.push({ "value": 0, "label": resx.get("Days30") });
        timespanSiteOptions.push({ "value": 1, "label": resx.get("Days60") });
        timespanSiteOptions.push({ "value": 2, "label": resx.get("Days90") });
        timespanSiteOptions.push({ "value": 3, "label": resx.get("Days180") });
        timespanSiteOptions.push({ "value": 4, "label": resx.get("Years1") });
        timespanSiteOptions.push({ "value": 5, "label": resx.get("Years2") });
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        if (props.apiTokenSettings) {
            this.setState({
                apiTokenSettings: props.apiTokenSettings,
            });
            return;
        }
        this.getSettings();
    }

    getUserTimespanOptions() {
        if (timespanUserOptions.length === 0) {
            const max = this.state.apiTokenSettings.MaximumSiteTimespan;
            timespanUserOptions = timespanSiteOptions.filter((item) => {
                return item.value <= max;
            });
        }
        return timespanUserOptions;
    }

    onSettingChange(key, event) {
        const { state, props } = this;
        let apiTokenSettings = Object.assign({}, state.apiTokenSettings);
        apiTokenSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            apiTokenSettings: apiTokenSettings,
        }, () => {
            timespanUserOptions = [];
        });
        props.dispatch(SecurityActions.apiTokenSettingsClientModified(apiTokenSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SecurityActions.updateApiTokenSettings(
                state.apiTokenSettings,
                () => {
                    util.utilities.notify(resx.get("ApiTokenSettingsUpdateSuccess"));
                    this.getSettings();
                },
                () => {
                    util.utilities.notifyError(resx.get("ApiTokenSettingsError"));
                }
            )
        );
    }

    onCancel() {
        util.utilities.confirm(
            resx.get("ApiTokenSettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                this.getSettings();
            }
        );
    }

    getSettings() {
        const { props } = this;
        props.dispatch(
            SecurityActions.getApiTokenSettings((data) => {
                let apiTokenSettings = Object.assign({}, data.Results.ApiTokenSettings);
                this.setState({
                    apiTokenSettings,
                });
            })
        );
    }

    testPositiveInt(value) {
        return /^([0-9]+)$/.test(value);
    }

     
    render() {
        const { state } = this;
        const isHost = util.settings.isHost;

        if (state.apiTokenSettings) {
            let warningBox = <div />;
            if (!state.apiTokenSettings.ApiTokensEnabled) {
                warningBox = (
                    <div className="warningBox">
                        <div className="warningText">{resx.get("ApiTokensDisabled.Help")}</div>
                    </div>
                );
            }
            return (
                <div id="apiTokenSettings-container">
                    {warningBox}
                    <InputGroup>
                        <div className="apiTokenSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plAllowApiTokens.Help")}
                                label={resx.get("plAllowApiTokens")}
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.apiTokenSettings.AllowApiTokens}
                                readOnly={!state.apiTokenSettings.ApiTokensEnabled}
                                onChange={this.onSettingChange.bind(this, "AllowApiTokens")}
                            />
                        </div>
                    </InputGroup>
                    {isHost && (
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("plMaximumSiteTimespan.Help")}
                                label={resx.get("plMaximumSiteTimespan")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Dropdown
                                options={timespanSiteOptions}
                                value={state.apiTokenSettings.MaximumSiteTimespan}
                                onSelect={(newVal) => {
                                    this.onSettingChange("MaximumSiteTimespan", newVal.value);
                                }}
                                enabled={state.apiTokenSettings.ApiTokensEnabled}
                            />
                        </InputGroup>
                    )}
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plUserTokenTimespan.Help")}
                            label={resx.get("plUserTokenTimespan")}
                        />
                        <Dropdown
                            options={this.getUserTimespanOptions()}
                            value={state.apiTokenSettings.UserTokenTimespan}
                            onSelect={(newVal) => {
                                this.onSettingChange("UserTokenTimespan", newVal.value);
                            }}
                            enabled={state.apiTokenSettings.AllowApiTokens}
                        />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.apiTokenSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.apiTokenSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}
                        >
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        } else return <div />;
    }
}

ApiTokenSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    apiTokenSettings: PropTypes.object,
    apiTokenSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        apiTokenSettings: state.security.apiTokenSettings,
        apiTokenSettingsClientModified: state.security.apiTokenSettingsClientModified,
    };
}

export default connect(mapStateToProps)(ApiTokenSettingsPanelBody);
