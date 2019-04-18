import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { siteBehavior as SiteBehaviorActions } from "../../actions";
import {
  InputGroup,
  Switch,
  GridSystem,
  Tooltip,
  Label,
  Button,
  SingleLineInputWithError,
  PagePicker,
  Dropdown
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class PrivacySettingsPanelBody extends Component {
  constructor() {
    super();
    this.state = {
      privacySettings: undefined
    };
  }

  loadData() {
    const { props } = this;
    props.dispatch(
      SiteBehaviorActions.getPrivacySettings(props.portalId, data => {
        this.setState({
          privacySettings: Object.assign({}, data.Settings)
        });
      })
    );
  }

  componentDidMount() {
    this.loadData();
  }

  componentDidUpdate(prevProps) {
    const { props } = this;
    if (props.privacySettings) {
      let portalIdChanged = false;
      let cultureCodeChanged = false;
      if (
        props.portalId === undefined ||
        prevProps.portalId === props.portalId
      ) {
        portalIdChanged = false;
      } else {
        portalIdChanged = true;
      }
      if (
        props.cultureCode === undefined ||
        prevProps.cultureCode === props.cultureCode
      ) {
        cultureCodeChanged = false;
      } else {
        cultureCodeChanged = true;
      }

      if (portalIdChanged || cultureCodeChanged) {
        this.loadData();
      }
    }
  }

  getUserDeleteOptions() {
    return [
      { label: resx.get("Off"), value: 0 },
      { label: resx.get("DataConsentUserDeleteManual"), value: 1 },
      { label: resx.get("DataConsentUserDelayedHardDelete"), value: 2 },
      { label: resx.get("DataConsentUserHardDelete"), value: 3 }
    ];
  }

  getTimeLapseMeasurements() {
    return [
      { value: "h", label: resx.get("Hours") },
      { value: "d", label: resx.get("Days") },
      { value: "w", label: resx.get("Weeks") }
    ];
  }

  onDataConsentResetTerms() {
    util.utilities.confirm(
      resx.get("DataConsentResetTerms.Confirm"),
      resx.get("Yes"),
      resx.get("No"),
      function onDataConsentResetTermsConfirm() {
        SiteBehaviorActions.resetTermsAgreement(
          {
            PortalId: this.state.privacySettings.PortalId
          },
          () => {
            util.utilities.notify(resx.get("DataConsentResetTerms.Completed"));
          }
        );
      }.bind(this)
    );
  }

  onSettingChange(key, event) {
    let { state, props } = this;
    let privacySettings = Object.assign({}, state.privacySettings);

    if (
      key === "DataConsentUserDeleteAction" ||
      key === "DataConsentDelayMeasurement"
    ) {
      privacySettings[key] = event.value;
    } else {
      privacySettings[key] =
        typeof event === "object" ? event.target.value : event;
    }

    this.setState({
      privacySettings: privacySettings
    });

    props.dispatch(
      SiteBehaviorActions.privacySettingsClientModified(privacySettings)
    );
  }

  onUpdate(event) {
    event.preventDefault();
    const { props, state } = this;

    props.dispatch(
      SiteBehaviorActions.updatePrivacySettings(
        state.privacySettings,
        () => {
          util.utilities.notify(resx.get("SettingsUpdateSuccess"));
          this.setState({
            privacySettings: Object.assign({}, this.state.privacySettings, {
              DataConsentResetTerms: false
            })
          });
        },
        () => {
          util.utilities.notifyError(resx.get("SettingsError"));
        }
      )
    );
  }

  onCancel() {
    const { props } = this;
    util.utilities.confirm(
      resx.get("SettingsRestoreWarning"),
      resx.get("Yes"),
      resx.get("No"),
      () => {
        props.dispatch(
          SiteBehaviorActions.getPrivacySettings(props.portalId, data => {
            this.setState({
              privacySettings: Object.assign({}, data.Settings)
            });
          })
        );
      }
    );
  }

  /* eslint-disable react/no-danger */
  render() {
    const { props, state } = this;
    const TabParameters = {
      portalId: props.portalId !== undefined ? props.portalId : -2,
      cultureCode: props.cultureCode || "",
      isMultiLanguage: false,
      excludeAdminTabs: false,
      roles: "",
      sortOrder: 0
    };
    let TabParameters_1 = Object.assign(Object.assign({}, TabParameters), {
      disabledNotSelectable: false
    });
    const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
    const columnOneLeft = state.privacySettings ? (
      <div key="column-one-left" className="left-column">
        <InputGroup>
          <Label
            labelType="inline"
            tooltipMessage={resx.get("plUpgrade.Help")}
            label={resx.get("plUpgrade")}
            extra={
              <Tooltip
                messages={[resx.get("GlobalSetting")]}
                type="global"
                style={{ float: "left", position: "static" }}
              />
            }
          />
          <Switch
            onText={resx.get("SwitchOn")}
            offText={resx.get("SwitchOff")}
            value={state.privacySettings.CheckUpgrade}
            onChange={this.onSettingChange.bind(this, "CheckUpgrade")}
          />
        </InputGroup>
        <InputGroup>
          <Label
            labelType="inline"
            tooltipMessage={resx.get("plDisplayCopyright.Help")}
            label={resx.get("plDisplayCopyright")}
            extra={
              <Tooltip
                messages={[resx.get("GlobalSetting")]}
                type="global"
                style={{ float: "left", position: "static" }}
              />
            }
          />
          <Switch
            onText={resx.get("SwitchOn")}
            offText={resx.get("SwitchOff")}
            value={state.privacySettings.DisplayCopyright}
            onChange={this.onSettingChange.bind(this, "DisplayCopyright")}
          />
        </InputGroup>
      </div>
    ) : (
      <div key="column-one-left" className="left-column" />
    );
    const columnOneRight = state.privacySettings ? (
      <div key="column-one-right" className="right-column">
        <InputGroup>
          <Label
            labelType="inline"
            tooltipMessage={resx.get("plImprovementProgram.Help")}
            label={resx.get("plImprovementProgram")}
            extra={
              <Tooltip
                messages={[resx.get("GlobalSetting")]}
                type="global"
                style={{ float: "left", position: "static" }}
              />
            }
            className="dnn-label-long"
          />
          <Switch
            onText={resx.get("SwitchOn")}
            offText={resx.get("SwitchOff")}
            value={state.privacySettings.DnnImprovementProgram}
            onChange={this.onSettingChange.bind(this, "DnnImprovementProgram")}
          />
        </InputGroup>
      </div>
    ) : (
      <div key="column-one-right" className="right-column" />
    );
    const columnTwoLeft = state.privacySettings ? (
      <div key="column-two-left" className="left-column">
        <InputGroup>
          <Label
            labelType="inline"
            tooltipMessage={resx.get("plShowCookieConsent.Help")}
            label={resx.get("plShowCookieConsent")}
          />
          <Switch
            onText={resx.get("SwitchOn")}
            offText={resx.get("SwitchOff")}
            value={state.privacySettings.ShowCookieConsent}
            onChange={this.onSettingChange.bind(this, "ShowCookieConsent")}
          />
        </InputGroup>
      </div>
    ) : (
      <div key="column-two-left" className="left-column" />
    );
    const columnTwoRight = state.privacySettings ? (
      <div key="column-two-right" className="right-column">
        <InputGroup>
          <Label
            tooltipMessage={resx.get("plCookieMoreLink.Help")}
            label={resx.get("plCookieMoreLink")}
          />
          <SingleLineInputWithError
            inputStyle={{ margin: "0" }}
            withLabel={false}
            enabled={state.privacySettings.ShowCookieConsent}
            error={false}
            value={state.privacySettings.CookieMoreLink}
            onChange={this.onSettingChange.bind(this, "CookieMoreLink")}
          />
        </InputGroup>
      </div>
    ) : (
      <div key="column-two-right" className="right-column" />
    );
    const hardDeleteDelay =
      state.privacySettings &&
      state.privacySettings.DataConsentUserDeleteAction === 2 ? (
        <div className="editor-row divider">
          <SingleLineInputWithError
            withLabel={true}
            style={{ float: "left", width: "47.5%", whiteSpace: "pre" }}
            label={resx.get("DataConsentDelay")}
            error={false}
            errorMessage={resx.get("DataConsentDelay.ErrorMessage")}
            value={state.privacySettings.DataConsentDelay}
            onChange={this.onSettingChange.bind(this, "DataConsentDelay")}
          />
          <div className="text-section">&nbsp; </div>
          <Dropdown
            style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
            options={this.getTimeLapseMeasurements()}
            value={state.privacySettings.DataConsentDelayMeasurement}
            onSelect={this.onSettingChange.bind(
              this,
              "DataConsentDelayMeasurement"
            )}
          />
        </div>
      ) : null;
    const columnThreeLeft = state.privacySettings ? (
      <div key="column-two-left" className="left-column">
        <InputGroup>
          <Label
            labelType="inline"
            tooltipMessage={resx.get("DataConsentActive.Help")}
            label={resx.get("DataConsentActive")}
          />
          <Switch
            onText={resx.get("SwitchOn")}
            offText={resx.get("SwitchOff")}
            value={state.privacySettings.DataConsentActive}
            onChange={this.onSettingChange.bind(this, "DataConsentActive")}
          />
        </InputGroup>
        <InputGroup>
          <Label
            tooltipMessage={resx.get("DataConsentConsentRedirect.Help")}
            label={resx.get("DataConsentConsentRedirect")}
          />
          <PagePicker
            serviceFramework={util.utilities.sf}
            style={{ width: "100%", zIndex: 5 }}
            selectedTabId={
              state.privacySettings.DataConsentConsentRedirect
                ? state.privacySettings.DataConsentConsentRedirect
                : -1
            }
            OnSelect={this.onSettingChange.bind(
              this,
              "DataConsentConsentRedirect"
            )}
            defaultLabel={
              state.privacySettings.DataConsentConsentRedirectName
                ? state.privacySettings.DataConsentConsentRedirectName
                : noneSpecifiedText
            }
            noneSpecifiedText={noneSpecifiedText}
            CountText={"{0} Results"}
            PortalTabsParameters={TabParameters_1}
          />
        </InputGroup>
        <InputGroup>
          <Label
            tooltipMessage={resx.get("DataConsentUserDeleteAction.Help")}
            label={resx.get("DataConsentUserDeleteAction")}
          />
          <Dropdown
            options={this.getUserDeleteOptions()}
            value={state.privacySettings.DataConsentUserDeleteAction}
            onSelect={this.onSettingChange.bind(
              this,
              "DataConsentUserDeleteAction"
            )}
          />
        </InputGroup>
        {hardDeleteDelay}
      </div>
    ) : (
      <div key="column-two-left" className="left-column" />
    );
    const columnThreeRight = state.privacySettings ? (
      <div key="column-two-right" className="right-column">
        <div class="warningBox">
          <div className="warningText">
            {resx.get("DataConsentResetTerms.Warning")}
          </div>
          <div className="warningButton">
            <Button
              type="secondary"
              onClick={this.onDataConsentResetTerms.bind(this)}
            >
              {resx.get("DataConsentResetTerms")}
            </Button>
          </div>
        </div>
      </div>
    ) : (
      <div key="column-two-right" className="right-column" />
    );

    return (
      <div className={styles.privacySettings}>
        <div className="sectionTitle">
          {resx.get("PrivacyCommunicationSettings")}
        </div>
        <GridSystem numberOfColumns={2}>
          {[columnOneLeft, columnOneRight]}
        </GridSystem>
        <div className="sectionTitle">
          {resx.get("PrivacyCookieConsentSettings")}
        </div>
        <GridSystem numberOfColumns={2}>
          {[columnTwoLeft, columnTwoRight]}
        </GridSystem>
        <div className="sectionTitle">{resx.get("DataConsentSettings")}</div>
        <GridSystem numberOfColumns={2}>
          {[columnThreeLeft, columnThreeRight]}
        </GridSystem>
        <div className="buttons-box">
          <Button
            disabled={!props.privacySettingsClientModified}
            type="secondary"
            onClick={this.onCancel.bind(this)}
          >
            {resx.get("Cancel")}
          </Button>
          <Button
            disabled={!props.privacySettingsClientModified}
            type="primary"
            onClick={this.onUpdate.bind(this)}
          >
            {resx.get("Save")}
          </Button>
        </div>
      </div>
    );
  }
}

PrivacySettingsPanelBody.propTypes = {
  dispatch: PropTypes.func.isRequired,
  tabIndex: PropTypes.number,
  privacySettings: PropTypes.object,
  privacySettingsClientModified: PropTypes.bool,
  portalId: PropTypes.number,
  cultureCode: PropTypes.string
};

function mapStateToProps(state) {
  return {
    tabIndex: state.pagination.tabIndex,
    privacySettings: state.siteBehavior.privacySettings,
    privacySettingsClientModified:
      state.siteBehavior.privacySettingsClientModified,
    portalId: state.siteInfo ? state.siteInfo.portalId : undefined
  };
}

export default connect(mapStateToProps)(PrivacySettingsPanelBody);
