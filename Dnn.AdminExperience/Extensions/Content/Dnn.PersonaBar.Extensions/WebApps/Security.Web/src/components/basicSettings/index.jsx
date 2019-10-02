import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { InputGroup, Dropdown, PagePicker, Switch, Label, Button } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let canEdit = false;
class BasicSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            basicLoginSettings: undefined
        };
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.BASIC_LOGIN_SETTINGS_EDIT;
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.basicLoginSettings) {
            this.setState({
                basicLoginSettings: props.basicLoginSettings
            });
            return;
        }
        props.dispatch(SecurityActions.getBasicLoginSettings(props.cultureCode, (data) => {
            let basicLoginSettings = Object.assign({}, data.Results.Settings);
            this.setState({
                basicLoginSettings
            });
        }));
    }

    getAuthProviderOptions() {
        let options = [];
        if (this.props.authProviders !== undefined) {
            options = this.props.authProviders.map((item) => {
                return { label: item.Name, value: item.Value };
            });
        }

        return options;
    }

    getAdminUserOptions() {
        let options = [];
        if (this.props.adminUsers !== undefined) {
            options = this.props.adminUsers.map((item) => {
                return { label: item.FullName, value: item.UserID };
            });
        }

        return options;
    }

    onSettingChange(key, event) {
        const {props, state} = this;

        let basicLoginSettings = Object.assign({}, state.basicLoginSettings);

        if (key === "RedirectAfterLoginTabId" || key === "RedirectAfterLogoutTabId") {
            if (basicLoginSettings[key] !== parseInt(event)) {
                basicLoginSettings[key] = event;
            }
            else {
                return;
            }
        }
        else if (key === "DefaultAuthProvider" || key === "PrimaryAdministratorId") {
            basicLoginSettings[key] = event.value;
        }
        else {
            basicLoginSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            basicLoginSettings: basicLoginSettings
        });
        props.dispatch(SecurityActions.basicLoginSettingsClientModified(basicLoginSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SecurityActions.updateBasicLoginSettings(state.basicLoginSettings, () => {
            util.utilities.notify(resx.get("BasicLoginSettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("BasicLoginSettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("LoginSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getBasicLoginSettings(props.cultureCode, (data) => {
                let basicLoginSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    basicLoginSettings
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {state} = this;
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        const RedirectAfterLogoutParameters = {
            portalId: -2,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            disabledNotSelectable: false,
            roles: "-1",
            sortOrder: 0
        };
        const RedirectAfterLoginParameters = {
            portalId: -2,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            disabledNotSelectable: false,
            roles: "1;-1",
            sortOrder: 0
        };

        if (state.basicLoginSettings) {
            return (
                <div className={styles.loginSettings}>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("DefaultAuthProvider.Help") }
                            label={resx.get("DefaultAuthProvider") } />
                        <Dropdown
                            options={this.getAuthProviderOptions() }
                            value={state.basicLoginSettings.DefaultAuthProvider}
                            onSelect={this.onSettingChange.bind(this, "DefaultAuthProvider") }
                            enabled={canEdit} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAdministrator.Help") }
                            label={resx.get("plAdministrator") } />
                        <Dropdown
                            options={this.getAdminUserOptions() }
                            value={state.basicLoginSettings.PrimaryAdministratorId}
                            onSelect={this.onSettingChange.bind(this, "PrimaryAdministratorId") }
                            enabled={canEdit} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("Redirect_AfterLogin.Help") }
                            label={resx.get("Redirect_AfterLogin") } />
                        <PagePicker
                            serviceFramework={util.utilities.sf}
                            style={{ width: "100%", zIndex: 2 }}
                            selectedTabId={state.basicLoginSettings.RedirectAfterLoginTabId}
                            OnSelect={this.onSettingChange.bind(this, "RedirectAfterLoginTabId") }
                            defaultLabel={state.basicLoginSettings.RedirectAfterLoginTabName !== "" ? state.basicLoginSettings.RedirectAfterLoginTabName : noneSpecifiedText}
                            noneSpecifiedText={noneSpecifiedText}
                            CountText={"{0} Results"}
                            PortalTabsParameters={RedirectAfterLoginParameters}
                            enabled={canEdit} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("Redirect_AfterLogout.Help") }
                            label={resx.get("Redirect_AfterLogout") } />
                        <PagePicker
                            serviceFramework={util.utilities.sf}
                            style={{ width: "100%", zIndex: 1 }}
                            selectedTabId={state.basicLoginSettings.RedirectAfterLogoutTabId}
                            OnSelect={this.onSettingChange.bind(this, "RedirectAfterLogoutTabId") }
                            defaultLabel={state.basicLoginSettings.RedirectAfterLogoutTabName !== "" ? state.basicLoginSettings.RedirectAfterLogoutTabName : noneSpecifiedText}
                            noneSpecifiedText={noneSpecifiedText}
                            CountText={"{0} Results"}
                            PortalTabsParameters={RedirectAfterLogoutParameters}
                            enabled={canEdit} />
                    </InputGroup>
                    <InputGroup>
                        <div className="loginSettings-row_switch" style={{ margin: "0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_RequireValidProfileAtLogin.Help") }
                                label={resx.get("Security_RequireValidProfileAtLogin") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicLoginSettings.RequireValidProfileAtLogin}
                                onChange={this.onSettingChange.bind(this, "RequireValidProfileAtLogin") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="loginSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_CaptchaLogin.Help") }
                                label={resx.get("Security_CaptchaLogin") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicLoginSettings.CaptchaLogin}
                                onChange={this.onSettingChange.bind(this, "CaptchaLogin") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="loginSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_CaptchaRetrivePassword.Help") }
                                label={resx.get("Security_CaptchaRetrivePassword") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicLoginSettings.CaptchaRetrivePassword}
                                onChange={this.onSettingChange.bind(this, "CaptchaRetrivePassword") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="loginSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_CaptchaChangePassword.Help") }
                                label={resx.get("Security_CaptchaChangePassword") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicLoginSettings.CaptchaChangePassword}
                                onChange={this.onSettingChange.bind(this, "CaptchaChangePassword") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="loginSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plHideLoginControl.Help") }
                                label={resx.get("plHideLoginControl") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicLoginSettings.HideLoginControl}
                                onChange={this.onSettingChange.bind(this, "HideLoginControl") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    {canEdit &&
                        <div className="buttons-box">
                            <Button
                                disabled={!this.props.basicLoginSettingsClientModified}
                                type="secondary"
                                onClick={this.onCancel.bind(this) }>
                                {resx.get("Cancel") }
                            </Button>
                            <Button
                                disabled={!this.props.basicLoginSettingsClientModified}
                                type="primary"
                                onClick={this.onUpdate.bind(this) }>
                                {resx.get("Save") }
                            </Button>
                        </div>
                    }
                </div>
            );
        }
        else return <div />;
    }
}

BasicSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    authProviders: PropTypes.array,
    adminUsers: PropTypes.array,
    basicLoginSettings: PropTypes.object,
    basicLoginSettingsClientModified: PropTypes.bool,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        authProviders: state.security.authProviders,
        adminUsers: state.security.adminUsers,
        basicLoginSettings: state.security.basicLoginSettings,
        basicLoginSettingsClientModified: state.security.basicLoginSettingsClientModified
    };
}

export default connect(mapStateToProps)(BasicSettingsPanelBody);