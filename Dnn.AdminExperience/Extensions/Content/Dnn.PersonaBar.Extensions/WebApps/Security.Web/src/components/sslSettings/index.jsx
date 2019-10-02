import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { 
    InputGroup,
    SingleLineInputWithError,
    Switch,
    Label,
    Button,
    Tooltip
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class SslSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            sslSettings: undefined
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.sslSettings) {
            this.setState({
                sslSettings: props.sslSettings
            });
            return;
        }
        this.getSslSettings();
    }

    onSettingChange(key, event) {
        const {state, props} = this;
        let sslSettings = Object.assign({}, state.sslSettings);
        sslSettings[key] = typeof (event) === "object" ? event.target.value : event;
        this.setState({
            sslSettings: sslSettings
        });
        props.dispatch(SecurityActions.sslSettingsClientModified(sslSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SecurityActions.updateSslSettings(state.sslSettings, () => {
            util.utilities.notify(resx.get("SslSettingsUpdateSuccess"));
            this.getSslSettings();
        }, () => {
            util.utilities.notifyError(resx.get("SslSettingsError"));
        }));
    }

    onCancel() {
        util.utilities.confirm(resx.get("SslSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            this.getSslSettings();
        });
    }

    getSslSettings() {
        const {props} = this;
        props.dispatch(SecurityActions.getSslSettings((data) => {
            let sslSettings = Object.assign({}, data.Results.Settings);
            this.setState({
                sslSettings
            });
        }));
    }

    /* eslint-disable react/no-danger */
    render() {
        const {state} = this;
        if (state.sslSettings) {
            return (
                <div className={styles.sslSettings}>
                    <InputGroup>
                        <div className="sslSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plSSLEnabled.Help")}
                                label={resx.get("plSSLEnabled")} />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.sslSettings.SSLEnabled}
                                onChange={this.onSettingChange.bind(this, "SSLEnabled")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="sslSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plSSLEnforced.Help")}
                                label={resx.get("plSSLEnforced")} />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.sslSettings.SSLEnforced}
                                onChange={this.onSettingChange.bind(this, "SSLEnforced")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plSSLURL.Help")}
                            label={resx.get("plSSLURL")} />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={false}
                            value={state.sslSettings.SSLURL}
                            onChange={this.onSettingChange.bind(this, "SSLURL")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plSTDURL.Help")}
                            label={resx.get("plSTDURL")} />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={false}
                            value={state.sslSettings.STDURL}
                            onChange={this.onSettingChange.bind(this, "STDURL")} />
                    </InputGroup>
                    {
                        /*eslint-disable eqeqeq*/
                        state.sslSettings.SSLOffloadHeader != undefined &&
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("plSSLOffload.Help")}
                                label={resx.get("plSSLOffload")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <SingleLineInputWithError
                                withLabel={false}
                                error={false}
                                value={state.sslSettings.SSLOffloadHeader}
                                onChange={this.onSettingChange.bind(this, "SSLOffloadHeader")} />
                        </InputGroup>
                    }
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
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

SslSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    sslSettings: PropTypes.object,
    sslSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        sslSettings: state.security.sslSettings,
        sslSettingsClientModified: state.security.sslSettingsClientModified
    };
}

export default connect(mapStateToProps)(SslSettingsPanelBody);