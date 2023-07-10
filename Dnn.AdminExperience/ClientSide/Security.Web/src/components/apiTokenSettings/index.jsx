import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
    Dropdown,
    InputGroup,
    SingleLineInputWithError,
    Switch,
    Label,
    Button,
    Tooltip,
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class ApiTokenSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            apiTokenSettings: undefined,
        };
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

    onSettingChange(key, event) {
        const { state, props } = this;
        let apiTokenSettings = Object.assign({}, state.apiTokenSettings);
        apiTokenSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            apiTokenSettings: apiTokenSettings,
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

    /* eslint-disable react/no-danger */
    render() {
        const { state } = this;
        const isHost = util.settings.isHost;
        const isAdmin = isHost || util.settings.isAdmin;

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
                <div className={styles.apiTokenSettings}>
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
                                onChange={this.onSettingChange.bind(this, "AllowApiTokens")}
                            />
                        </div>
                    </InputGroup>
                    {isHost && (
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("plMaxApiTokenDuration.Help")}
                                label={resx.get("plMaxApiTokenDuration")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <div className="half-input-left">
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={!this.testPositiveInt(state.apiTokenSettings.MaximumTimespan)}
                                    errorMessage={resx.get("MaxApiTokenDuration.Error")}
                                    value={state.apiTokenSettings.MaximumTimespan}
                                    onChange={(e) => this.onSettingChange("MaximumTimespan", e)} />
                            </div>
                            <div className="half-input-right">
                                <Dropdown
                                    options={[
                                        { label: resx.get("OptDays"), value: "d" },
                                        { label: resx.get("OptWeeks"), value: "w" },
                                        { label: resx.get("OptMonths"), value: "M" },
                                        { label: resx.get("OptYears"), value: "y" },
                                    ]}
                                    value={state.apiTokenSettings.MaximumTimespanMeasure}
                                    onSelect={(newVal) => {
                                        this.onSettingChange("MaximumTimespanMeasure", newVal.value);
                                    }}
                                    enabled={true}
                                />
                            </div>
                        </InputGroup>
                    )}
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.apiTokenSettingsClientModified}
                            type="secondary"
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
