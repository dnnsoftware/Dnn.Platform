import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridSystem, Button } from "@dnnsoftware/dnn-react-common";
import RadioButtonBlock from "../common/RadioButtonBlock";
import EditBlock from "../common/EditBlock";
import EditPwdBlock from "../common/EditPwdBlock";
import SwitchBlock from "../common/SwitchBlock";
import DropdownBlock from "../common/DropdownBlock";
import localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import SmtpServerTabActions from "../../actions/smtpServerTab";
import utils from "../../utils";

class SmtpServer extends Component {
  componentDidMount() {
    const loadSettings = () => {
      const { props } = this;

      const selectedSmtpSettings = this.getSelectedSmtpSettings();

      this.onChangeAuthProvider(
        { value: selectedSmtpSettings.authProvider },
        true,
      );
    };

    this.props.onRetrieveSmtpServerInfo(loadSettings.bind(this));
    this.props.onRetrieveAuthProviders(loadSettings.bind(this));
  }

  UNSAFE_componentWillReceiveProps(newProps) {
    const { props } = this;

    if (
      this.props.infoMessage !== newProps.infoMessage &&
      newProps.infoMessage
    ) {
      utils.notify(newProps.infoMessage);
    }

    if (
      this.props.errorMessage !== newProps.errorMessage &&
      newProps.errorMessage
    ) {
      utils.notifyError(newProps.errorMessage);
    }

    const selectedSmtpSettings = this.getSelectedSmtpSettings();
    if (
      newProps.providerChanged &&
      selectedSmtpSettings.smtpAuthentication === "3"
    ) {
      this.props.onRetrieveAuthProviders();
    }
  }

  onChangeSmtpServerMode(mode) {
    const loadSettings = () => {
      const { props } = this;

      const selectedSmtpSettings = this.getSelectedSmtpSettings();
      this.onChangeAuthProvider(
        { value: selectedSmtpSettings.authProvider },
        true,
      );
    };

    this.props.onChangeSmtpServerMode(mode, loadSettings.bind(this));
  }

  onChangeAuthenticationMode(authentication) {
    this.props.onChangeSmtpAuthentication(authentication);
  }

  onChangeSmtpEnableSsl(enabled) {
    this.props.onChangeSmtpConfigurationValue("enableSmtpSsl", enabled);
  }

  onChangeField(key, event) {
    this.props.onChangeSmtpConfigurationValue(key, event.target.value);
  }

  onChangeAuthProvider(provider, passCheck) {
    const { props } = this;

    const authProvider = this.getSelectedAuthProviders()
      .filter(function (item) {
        return item.name === provider.value;
      })
      .pop();

    this.props.onChangeSmtpConfigurationValue(
      "authProvider",
      provider.value,
      passCheck,
    );

    //initialize settings
    let settings = [];
    if (typeof authProvider !== "undefined") {
      for (let i = 0; i < authProvider.settings.length; i++) {
        const setting = authProvider.settings[i];
        settings.push({
          name: setting.name,
          value: setting.value,
          label: setting.label,
          help: setting.help,
          isSecure: setting.isSecure,
          isRequired: setting.isRequired,
        });
      }
    }
    this.props.onChangeSmtpConfigurationValue(
      "authProviderSettings",
      settings,
      passCheck,
    );
  }

  onChangeAuthSetting(name, event) {
    const { props } = this;

    const selectedSmtpSettings = this.getSelectedSmtpSettings();
    const settings = selectedSmtpSettings.authProviderSettings;

    let newSettings = [];
    for (let i = 0; i < settings.length; i++) {
      newSettings.push({
        name: settings[i].name,
        value:
          settings[i].name === name ? event.target.value : settings[i].value,
        label: settings[i].label,
        help: settings[i].help,
        isSecure: settings[i].isSecure,
        isRequired: settings[i].isRequired,
      });
    }

    props.onChangeSmtpConfigurationValue("authProviderSettings", newSettings);
  }

  onSave() {
    const { props } = this;

    if (this.areThereValidationError()) {
      return;
    }

    const smtpSettings =
      props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser()
        ? props.smtpServerInfo.host
        : props.smtpServerInfo.site;

    const updateRequest = {
      smtpServerMode: props.smtpServerInfo.smtpServerMode,
      smtpServer: smtpSettings.smtpServer,
      smtpConnectionLimit: smtpSettings.smtpConnectionLimit,
      smtpMaxIdleTime: smtpSettings.smtpMaxIdleTime,
      smtpAuthentication: smtpSettings.smtpAuthentication,
      smtpUsername: smtpSettings.smtpUserName,
      smtpPassword: smtpSettings.smtpPassword,
      smtpHostEmail: smtpSettings.smtpHostEmail,
      enableSmtpSsl: smtpSettings.enableSmtpSsl,
      authProvider: smtpSettings.authProvider,
      AuthProviderSettings: this.toDictionary(
        smtpSettings.authProviderSettings,
      ),
      messageSchedulerBatchSize:
        props.smtpServerInfo.host.messageSchedulerBatchSize,
    };
    props.onUpdateSmtpServerSettings(updateRequest, this.onSettingsUpdated);
  }

  onSettingsUpdated(result) {}

  toDictionary(settings) {
    let dict = {};
    settings = settings || [];
    for (let i = 0; i < settings.length; i++) {
      dict[settings[i].name] = settings[i].value;
    }

    return dict;
  }

  getSelectedSmtpSettings() {
    const { props } = this;

    const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
    const selectedSmtpSettings =
      (areGlobalSettings
        ? props.smtpServerInfo.host
        : props.smtpServerInfo.site) || {};
    return selectedSmtpSettings;
  }

  getSelectedAuthProviders() {
    const { props } = this;

    if (
      typeof props.smtpServerInfo === "undefined" ||
      typeof props.authProviders === "undefined"
    ) {
      return [];
    }

    const isGlobal = props.smtpServerInfo.smtpServerMode === "h";
    return (
      (isGlobal ? props.authProviders.host : props.authProviders.site) || []
    );
  }

  areThereValidationError() {
    let areErrors = false;
    const errors = this.props.errors;
    for (let prop in errors) {
      if (errors[prop]) {
        return true;
      }
    }

    return areErrors;
  }

  onTestSmtpSettings() {
    const { props } = this;

    if (this.areThereValidationError()) {
      return;
    }

    let smtpSettings = {};
    if (props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser()) {
      smtpSettings = props.smtpServerInfo.host;
    }
    if (props.smtpServerInfo.smtpServerMode === "p") {
      smtpSettings = props.smtpServerInfo.site;
    }
    const authProvider =
      this.getSelectedAuthProviders()
        .filter(function (item) {
          return item.name === smtpSettings.authProvider;
        })
        .pop() || {};

    if (
      smtpSettings.smtpAuthentication === "3" &&
      (props.isDirty || !authProvider.isAuthorized)
    ) {
      utils.notifyError(localization.get("OAuthConfigurationNotSaved"));
      return;
    }

    const sendEmailRequest = {
      smtpServerMode: props.smtpServerInfo.smtpServerMode,
      smtpServer: smtpSettings.smtpServer,
      smtpAuthentication: smtpSettings.smtpAuthentication,
      smtpUsername: smtpSettings.smtpUserName,
      smtpPassword: smtpSettings.smtpPassword,
      enableSmtpSsl: smtpSettings.enableSmtpSsl,
      authProvider: smtpSettings.authProvider,
    };
    props.onSendTestEmail(sendEmailRequest);
  }

  onCompleteAuthorize() {
    const { props } = this;

    const selectedSmtpSettings = this.getSelectedSmtpSettings();
    const authProvider = this.getSelectedAuthProviders()
      .filter(function (item) {
        return item.name === selectedSmtpSettings.authProvider;
      })
      .pop();

    if (authProvider) {
      const myWindow = this.popupWindow("about:blank", "OAuth", 1090, 600);
      myWindow.location = authProvider.authorizeUrl;

      const intervalHandler = window.setInterval(
        (() => {
          if (myWindow.window === null || myWindow.closed === true) {
            this.props.onRetrieveAuthProviders();
            window.clearInterval(intervalHandler);
          }
        }).bind(this),
        1000,
      );
    }

    return false;
  }

  popupWindow(url, title, w, h) {
    const left = screen.width / 2 - w / 2;
    const top = screen.height / 2 - h / 2;
    return window.open(
      url,
      title,
      "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" +
        w +
        ", height=" +
        h +
        ", top=" +
        top +
        ", left=" +
        left,
    );
  }

  getSmtpServerOptions() {
    return [
      {
        label: localization.get("GlobalSmtpHostSetting"),
        value: "h",
      },
      {
        label: localization
          .get("SiteSmtpHostSetting")
          .replace("{0}", this.props.smtpServerInfo.portalName || ""),
        value: "p",
      },
    ];
  }

  getSmtpAuthenticationOptions() {
    const hasAuthProviders = this.getSelectedAuthProviders().length > 0;
    let options = [
      {
        label: localization.get("SMTPAnonymous"),
        value: "0",
      },
      {
        label: localization.get("SMTPBasic"),
        value: "1",
      },
      {
        label: localization.get("SMTPNTLM"),
        value: "2",
      },
    ];

    if (hasAuthProviders) {
      options.push({
        label: localization.get("SMTPOAUTH"),
        value: "3",
      });
    }

    return options;
  }

  renderAuthSettings() {
    const { props } = this;

    const authProvidersOptions = this.getSelectedAuthProviders().map(
      function (item) {
        return { label: item.localizedName, value: item.name };
      },
    );

    const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
    const selectedSmtpSettings = this.getSelectedSmtpSettings();
    const authProvider = this.getSelectedAuthProviders()
      .filter(function (item) {
        return item.name === selectedSmtpSettings.authProvider;
      })
      .pop();

    let settingFields = null;
    if (typeof authProvider !== "undefined") {
      const settings = selectedSmtpSettings.authProviderSettings || [];

      settingFields = settings.map(
        function (setting) {
          return (
            <EditBlock
              key={setting.name}
              label={setting.label}
              tooltip={setting.help}
              value={setting.value}
              isGlobal={areGlobalSettings}
              onChange={this.onChangeAuthSetting.bind(this, setting.name)}
              type={setting.isSecure ? "password" : "text"}
            />
          );
        }.bind(this),
      );
    }

    const settingCompleted =
      (selectedSmtpSettings.authProviderSettings || []).filter(
        (i) => i.isRequired && (i.value || "") === "",
      ).length === 0;
    return (
      <div>
        <div style={{ paddingBottom: "22px", float: "left", width: "100%" }}>
          <DropdownBlock
            tooltip={localization.get("SmtpTab_OAuthProviders.Help")}
            label={localization.get("SmtpTab_OAuthProviders")}
            options={authProvidersOptions}
            value={selectedSmtpSettings.authProvider}
            onSelect={this.onChangeAuthProvider.bind(this)}
          />
        </div>
        {settingFields}
        {authProvider &&
          !authProvider.isAuthorized &&
          authProvider.authorizeUrl &&
          settingCompleted &&
          !props.isDirty && (
            <div className="warningBox authorize-box">
              <div className="warningText">
                {localization.get("CompleteAuthorize")}
              </div>
              <div className="warningButton">
                <Button
                  type="secondary"
                  onClick={this.onCompleteAuthorize.bind(this)}
                >
                  {localization.get("Authorize")}
                </Button>
              </div>
            </div>
          )}
        {authProvider && authProvider.isAuthorized && !props.isDirty && (
          <div className="warningBox authorize-box success">
            <div className="warningText">
              {localization.get("AuthorizeCompleted")}
            </div>
          </div>
        )}
      </div>
    );
  }

  render() {
    const { props } = this;

    const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
    const selectedSmtpSettings = this.getSelectedSmtpSettings();
    const credentialVisible = selectedSmtpSettings.smtpAuthentication === "1";
    const oauthEnabled = selectedSmtpSettings.smtpAuthentication === "3";
    const smtpSettingsVisible = utils.isHostUser() || !areGlobalSettings;

    if (props.smtpServerInfo.hideCoreSettings) {
      return (
        <div className="dnn-servers-info-panel-big smtpServerSettingsTab">
          <div className="warningBox">
            <div className="warningText">
              {localization.get("NonCoreMailProvider")}
            </div>
          </div>
        </div>
      );
    }

    return (
      <div className="dnn-servers-info-panel-big smtpServerSettingsTab">
        <GridSystem>
          <div className="leftPane">
            <div className="tooltipAdjustment border-bottom">
              <RadioButtonBlock
                options={this.getSmtpServerOptions()}
                label={localization.get("plSMTPMode")}
                tooltip={localization.get("plSMTPMode.Help")}
                onChange={this.onChangeSmtpServerMode.bind(this)}
                value={props.smtpServerInfo.smtpServerMode}
              />
            </div>
            <div className="tooltipAdjustment">
              {smtpSettingsVisible && (
                <div>
                  <EditBlock
                    label={localization.get("plSMTPServer")}
                    tooltip={localization.get("plSMTPServer.Help")}
                    value={selectedSmtpSettings.smtpServer}
                    isGlobal={areGlobalSettings}
                    onChange={this.onChangeField.bind(this, "smtpServer")}
                    error={props.errors["smtpServer"]}
                  />

                  <EditBlock
                    label={localization.get("plConnectionLimit")}
                    tooltip={localization.get("plConnectionLimit.Help")}
                    value={selectedSmtpSettings.smtpConnectionLimit}
                    isGlobal={areGlobalSettings}
                    onChange={this.onChangeField.bind(
                      this,
                      "smtpConnectionLimit",
                    )}
                    error={props.errors["smtpConnectionLimit"]}
                  />

                  <EditBlock
                    label={localization.get("plMaxIdleTime")}
                    tooltip={localization.get("plMaxIdleTime.Help")}
                    value={selectedSmtpSettings.smtpMaxIdleTime}
                    isGlobal={areGlobalSettings}
                    onChange={this.onChangeField.bind(this, "smtpMaxIdleTime")}
                    error={props.errors["smtpMaxIdleTime"]}
                  />
                </div>
              )}
              {smtpSettingsVisible && areGlobalSettings && (
                <EditBlock
                  label={localization.get("plBatch")}
                  tooltip={localization.get("plBatch.Help")}
                  value={props.smtpServerInfo.host.messageSchedulerBatchSize}
                  isGlobal={areGlobalSettings}
                  onChange={this.onChangeField.bind(
                    this,
                    "messageSchedulerBatchSize",
                  )}
                  error={props.errors["messageSchedulerBatchSize"]}
                />
              )}
            </div>
          </div>
          <div className="rightPane">
            {smtpSettingsVisible && (
              <div className="tooltipAdjustment border-bottom smtp-authentication-mode">
                <RadioButtonBlock
                  options={this.getSmtpAuthenticationOptions()}
                  label={localization.get("plSMTPAuthentication")}
                  tooltip={localization.get("plSMTPAuthentication.Help")}
                  onChange={this.onChangeAuthenticationMode.bind(this)}
                  value={selectedSmtpSettings.smtpAuthentication || "0"}
                  isGlobal={areGlobalSettings}
                />
              </div>
            )}
            {smtpSettingsVisible && credentialVisible && (
              <div className="tooltipAdjustment">
                <EditBlock
                  label={localization.get("plSMTPUsername")}
                  tooltip={localization.get("plSMTPUsername.Help")}
                  value={selectedSmtpSettings.smtpUserName}
                  isGlobal={areGlobalSettings}
                  onChange={this.onChangeField.bind(this, "smtpUserName")}
                  error={props.errors["smtpUserName"]}
                />

                <EditPwdBlock
                  label={localization.get("plSMTPPassword")}
                  changeButtonText={localization.get("Change")}
                  tooltip={localization.get("plSMTPPassword.Help")}
                  value={selectedSmtpSettings.smtpPassword}
                  isGlobal={areGlobalSettings}
                  onChange={this.onChangeField.bind(this, "smtpPassword")}
                  onClear={() =>
                    props.onChangeSmtpConfigurationValue("smtpPassword", "")
                  }
                  error={props.errors["smtpPassword"]}
                />
              </div>
            )}
            {smtpSettingsVisible && !oauthEnabled && (
              <div
                className="tooltipAdjustment border-bottom"
                style={{ paddingBottom: "22px" }}
              >
                <SwitchBlock
                  label={localization.get("plSMTPEnableSSL")}
                  onText={localization.get("SwitchOn")}
                  offText={localization.get("SwitchOff")}
                  tooltip={localization.get("plSMTPEnableSSL.Help")}
                  value={selectedSmtpSettings.enableSmtpSsl}
                  onChange={this.onChangeSmtpEnableSsl.bind(this)}
                  isGlobal={areGlobalSettings}
                />
              </div>
            )}
            {oauthEnabled && (
              <div
                className="tooltipAdjustment border-bottom"
                style={{ paddingBottom: "22px" }}
              >
                {this.renderAuthSettings()}
              </div>
            )}
            {smtpSettingsVisible && areGlobalSettings && (
              <EditBlock
                label={localization.get("plHostEmail")}
                tooltip={localization.get("plHostEmail.Help")}
                value={selectedSmtpSettings.smtpHostEmail}
                isGlobal={true}
                onChange={this.onChangeField.bind(this, "smtpHostEmail")}
                error={props.errors["smtpHostEmail"]}
              />
            )}
          </div>
        </GridSystem>
        <div className="clear" />
        <div className="buttons-panel">
          <Button type="secondary" onClick={this.onTestSmtpSettings.bind(this)}>
            {localization.get("EmailTest")}
          </Button>
          <Button
            type="primary"
            onClick={this.onSave.bind(this)}
            disabled={!props.isDirty}
          >
            {localization.get("SaveButtonText")}
          </Button>
        </div>
      </div>
    );
  }
}

SmtpServer.propTypes = {
  smtpServerInfo: PropTypes.object.isRequired,
  authProviders: PropTypes.object,
  authProvider: PropTypes.string,
  errorMessage: PropTypes.string,
  onRetrieveSmtpServerInfo: PropTypes.func.isRequired,
  onRetrieveAuthProviders: PropTypes.func.isRequired,
  onChangeSmtpServerMode: PropTypes.func.isRequired,
  onChangeSmtpAuthentication: PropTypes.func.isRequired,
  onChangeSmtpConfigurationValue: PropTypes.func.isRequired,
  onUpdateSmtpServerSettings: PropTypes.func.isRequired,
  infoMessage: PropTypes.string,
  onSendTestEmail: PropTypes.func.isRequired,
  errors: PropTypes.array,
  isDirty: PropTypes.bool,
};

function mapStateToProps(state) {
  return {
    smtpServerInfo: state.smtpServer.smtpServerInfo,
    authProviders: state.smtpServer.authProviders,
    errorMessage: state.smtpServer.errorMessage,
    infoMessage: state.smtpServer.infoMessage,
    success: state.smtpServer.success,
    providerChanged: state.smtpServer.providerChanged,
    errors: state.smtpServer.errors,
    isDirty: state.smtpServer.isDirty,
  };
}

function mapDispatchToProps(dispatch) {
  return {
    ...bindActionCreators(
      {
        onRetrieveSmtpServerInfo: SmtpServerTabActions.loadSmtpServerInfo,
        onRetrieveAuthProviders: SmtpServerTabActions.loadAuthProviders,
        onChangeSmtpServerMode: SmtpServerTabActions.changeSmtpServerMode,
        onChangeSmtpAuthentication:
          SmtpServerTabActions.changeSmtpAuthentication,
        onChangeSmtpConfigurationValue:
          SmtpServerTabActions.changeSmtpConfigurationValue,
        onUpdateSmtpServerSettings:
          SmtpServerTabActions.updateSmtpServerSettings,
        onSendTestEmail: SmtpServerTabActions.sendTestEmail,
      },
      dispatch,
    ),
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(SmtpServer);
