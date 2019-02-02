import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import { InputGroup, Switch, GridSystem, Dropdown, Label, Button } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let timeIntervalOptions = [];
let recipientLimitOptions = [];

class MessagingSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            messagingSettings: undefined
        };
    }

    loadData() {
        const {props} = this;
        if (props.messagingSettings) {
            let portalIdChanged = false;
            let cultureCodeChanged = false;

            if (props.portalId === undefined || props.messagingSettings.PortalId === props.portalId) {
                portalIdChanged = false;
            }
            else {
                portalIdChanged = true;
            }

            if (props.cultureCode === undefined || props.messagingSettings.CultureCode === props.cultureCode) {
                cultureCodeChanged = false;
            }
            else {
                cultureCodeChanged = true;
            }

            if (portalIdChanged || cultureCodeChanged) {
                return true;
            }
            else return false;
        }
        else {
            return true;
        }
    }

    componentDidMount() {
        const { props} = this;
        if (!this.loadData()) {
            this.setState({
                messagingSettings: props.messagingSettings
            });
            return;
        }

        timeIntervalOptions = [];
        timeIntervalOptions.push({ "value": 0, "label": "0" });
        timeIntervalOptions.push({ "value": 0.5, "label": "0.5" });
        timeIntervalOptions.push({ "value": 1, "label": "1" });
        timeIntervalOptions.push({ "value": 2, "label": "2" });
        timeIntervalOptions.push({ "value": 3, "label": "3" });
        timeIntervalOptions.push({ "value": 4, "label": "4" });
        timeIntervalOptions.push({ "value": 5, "label": "5" });
        timeIntervalOptions.push({ "value": 6, "label": "6" });
        timeIntervalOptions.push({ "value": 7, "label": "7" });
        timeIntervalOptions.push({ "value": 8, "label": "8" });
        timeIntervalOptions.push({ "value": 9, "label": "9" });
        timeIntervalOptions.push({ "value": 10, "label": "10" });

        recipientLimitOptions = [];
        recipientLimitOptions.push({ "value": 1, "label": "1" });
        recipientLimitOptions.push({ "value": 5, "label": "5" });
        recipientLimitOptions.push({ "value": 10, "label": "10" });
        recipientLimitOptions.push({ "value": 15, "label": "15" });
        recipientLimitOptions.push({ "value": 25, "label": "25" });
        recipientLimitOptions.push({ "value": 50, "label": "50" });
        recipientLimitOptions.push({ "value": 75, "label": "75" });
        recipientLimitOptions.push({ "value": 100, "label": "100" });

        props.dispatch(SiteBehaviorActions.getMessagingSettings(props.portalId, (data) => {
            this.setState({
                messagingSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let messagingSettings = Object.assign({}, state.messagingSettings);

        if (key === "ThrottlingInterval" || key === "RecipientLimit") {
            messagingSettings[key] = event.value;
        }
        else {
            messagingSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            messagingSettings: messagingSettings
        });

        props.dispatch(SiteBehaviorActions.messagingSettingsClientModified(messagingSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteBehaviorActions.updateMessagingSettings(state.messagingSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getMessagingSettings(props.portalId, (data) => {
                this.setState({
                    messagingSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {state} = this;
        if (state.messagingSettings) {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <div className="messagingSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plDisablePrivateMessage.Help")}
                            label={resx.get("plDisablePrivateMessage")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.messagingSettings.DisablePrivateMessage}
                            onChange={this.onSettingChange.bind(this, "DisablePrivateMessage")}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plMsgThrottlingInterval.Help")}
                        label={resx.get("plMsgThrottlingInterval")}
                    />
                    <Dropdown
                        options={timeIntervalOptions}
                        value={state.messagingSettings.ThrottlingInterval}
                        onSelect={this.onSettingChange.bind(this, "ThrottlingInterval")}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plMsgRecipientLimit.Help")}
                        label={resx.get("plMsgRecipientLimit")}
                    />
                    <Dropdown
                        options={recipientLimitOptions}
                        value={state.messagingSettings.RecipientLimit}
                        onSelect={this.onSettingChange.bind(this, "RecipientLimit")}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                <InputGroup>
                    <div className="messagingSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plMsgProfanityFilters.Help")}
                            label={resx.get("plMsgProfanityFilters")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.messagingSettings.ProfanityFilters}
                            onChange={this.onSettingChange.bind(this, "ProfanityFilters")}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="messagingSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plIncludeAttachments.Help")}
                            label={resx.get("plIncludeAttachments")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.messagingSettings.IncludeAttachments}
                            onChange={this.onSettingChange.bind(this, "IncludeAttachments")}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="messagingSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plMsgAllowAttachments.Help")}
                            label={resx.get("plMsgAllowAttachments")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.messagingSettings.AllowAttachments}
                            onChange={this.onSettingChange.bind(this, "AllowAttachments")}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="messagingSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plMsgSendEmail.Help")}
                            label={resx.get("plMsgSendEmail")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.messagingSettings.SendEmail}
                            onChange={this.onSettingChange.bind(this, "SendEmail")}
                        />
                    </div>
                </InputGroup>
            </div>;

            return (
                <div className={styles.messagingSettings}>
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.messagingSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.messagingSettingsClientModified}
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

MessagingSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    messagingSettings: PropTypes.object,
    messagingSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        messagingSettings: state.siteBehavior.messagingSettings,
        messagingSettingsClientModified: state.siteBehavior.messagingSettingsClientModified
    };
}

export default connect(mapStateToProps)(MessagingSettingsPanelBody);