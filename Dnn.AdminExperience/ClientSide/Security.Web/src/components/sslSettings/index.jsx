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
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";

class SslSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            sslSettings: undefined,
        };
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        if (props.sslSettings) {
            this.setState({
                sslSettings: props.sslSettings,
            });
            return;
        }
        this.getSslSettings();
    }

    onSettingChange(key, event) {
        const { state, props } = this;
        let sslSettings = Object.assign({}, state.sslSettings);
        sslSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            sslSettings: sslSettings,
        });
        props.dispatch(SecurityActions.sslSettingsClientModified(sslSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SecurityActions.updateSslSettings(
                state.sslSettings,
                () => {
                    util.utilities.notify(resx.get("SslSettingsUpdateSuccess"));
                    this.getSslSettings();
                },
                () => {
                    util.utilities.notifyError(resx.get("SslSettingsError"));
                }
            )
        );
    }

    onCancel() {
        util.utilities.confirm(
            resx.get("SslSettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                this.getSslSettings();
            }
        );
    }

    onSetAllPagesSecure() {
        util.utilities.confirm(
            resx.get("SslSetAllPagesSecure.Confirm"),
            resx.get("Yes"),
            resx.get("No"),
            function onSetAllPagesSecureConfirm() {
                this.props.dispatch(
                    SecurityActions.updateSslSettings(
                        this.state.sslSettings,
                        () => {
                            SecurityActions.setAllPagesSecure(() => {
                                util.utilities.notify(
                                    resx.get("SslSetAllPagesSecure.Completed")
                                );
                                this.getSslSettings();
                            });
                        },
                        () => {
                            util.utilities.notifyError(resx.get("SslSettingsError"));
                        }
                    )
                );
            }.bind(this)
        );
    }

    getSslSettings() {
        const { props } = this;
        props.dispatch(
            SecurityActions.getSslSettings((data) => {
                let sslSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    sslSettings,
                });
            })
        );
    }

     
    render() {
        const { state } = this;
        if (state.sslSettings) {
            let warningBox = <div />;
            switch (state.sslSettings.SSLSetup) {
                case 0:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">{resx.get("SslOff.Help")}</div>
                        </div>
                    );
                    break;
                case 1:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">{resx.get("SslOn.Help")}</div>
                        </div>
                    );
                    break;
                case 2:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">
                                {resx
                                    .get("SslAdvanced.Help")
                                    .replace(
                                        "[NumberOfSecureTabs]",
                                        state.sslSettings.NumberOfSecureTabs.toString()
                                    )
                                    .replace(
                                        "[NumberOfNonSecureTabs]",
                                        state.sslSettings.NumberOfNonSecureTabs.toString()
                                    )}
                            </div>
                            <div className="warningButton">
                                <Button
                                    type="secondary"
                                    onClick={this.onSetAllPagesSecure.bind(this)}
                                >
                                    {resx.get("SslSetAllPagesSecure")}
                                </Button>
                            </div>
                        </div>
                    );
                    break;
            }
            return (
                <div className={styles.sslSettings}>
                    <InputGroup>
                        <Dropdown
                            options={[
                                { label: resx.get("SslOff"), value: "0" },
                                { label: resx.get("SslOn"), value: "1" },
                                { label: resx.get("SslAdvanced"), value: "2" },
                            ]}
                            value={state.sslSettings.SSLSetup.toString()}
                            onSelect={(newVal) => {
                                let sslSettings = state.sslSettings;
                                sslSettings.SSLSetup = parseInt(newVal.value);
                                this.setState({
                                    sslSettings,
                                });
                                this.props.dispatch(
                                    SecurityActions.sslSettingsClientModified(sslSettings)
                                );
                            }}
                            enabled={true}
                        />
                    </InputGroup>
                    {warningBox}
                    {Number.parseInt(state.sslSettings.SSLSetup) === 2 && (
                        <>
                            <InputGroup>
                                <div className="sslSettings-row_switch">
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("plSSLEnforced.Help")}
                                        label={resx.get("plSSLEnforced")}
                                    />
                                    <Switch
                                        onText={resx.get("SwitchOn")}
                                        offText={resx.get("SwitchOff")}
                                        value={state.sslSettings.SSLEnforced}
                                        onChange={this.onSettingChange.bind(this, "SSLEnforced")}
                                    />
                                </div>
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSSLURL.Help")}
                                    label={resx.get("plSSLURL")}
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.SSLURL}
                                    onChange={this.onSettingChange.bind(this, "SSLURL")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSTDURL.Help")}
                                    label={resx.get("plSTDURL")}
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.STDURL}
                                    onChange={this.onSettingChange.bind(this, "STDURL")}
                                />
                            </InputGroup>
                        </>
                    )}
                    {
                        /*eslint-disable eqeqeq*/
                        state.sslSettings.SSLOffloadHeader != undefined && state.sslSettings.SSLSetup != 1 && (
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSSLOffload.Help")}
                                    label={resx.get("plSSLOffload")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />
                                    }
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.SSLOffloadHeader}
                                    onChange={this.onSettingChange.bind(this, "SSLOffloadHeader")}
                                />
                            </InputGroup>
                        )
                    }
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
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

SslSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    sslSettings: PropTypes.object,
    sslSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        sslSettings: state.security.sslSettings,
        sslSettingsClientModified: state.security.sslSettingsClientModified,
    };
}

export default connect(mapStateToProps)(SslSettingsPanelBody);
