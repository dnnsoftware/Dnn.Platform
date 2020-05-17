import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { siteBehavior as SiteBehaviorActions } from "../../actions";
import {
  Button,
  Dropdown,
  GridSystem,
  InputGroup,
  Label,
  MultiLineInputWithError
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

function getError(errors) {
  let hasError = false;
  Object.keys(errors).forEach(key => {
    if (errors[key] === true) {
      hasError = true;
    }
  });
  return hasError;
}

let isHost = false;

class MoreSettingsPanelBody extends Component {
  constructor() {
    super();
    this.state = {
      otherSettings: undefined,
      siteBehaviorExtrasRendered: true,
      errorInSave: false,
      whitelistOption: 0
    };
    isHost = util.settings.isHost;
  }

  loadData() {
    const { props } = this;
    props.dispatch(
      SiteBehaviorActions.getOtherSettings(props.portalId, data => {
        var whitelistOption = 1;
        if (
          data.Settings.AllowedExtensionsWhitelist ==
          data.Settings.HostAllowedExtensionsWhitelists
        ) {
          whitelistOption = 0;
        } else if (
          data.Settings.AllowedExtensionsWhitelist == data.Settings.ImageExtensionsList
        ) {
          whitelistOption = 2;
        }
        this.setState({
          otherSettings: Object.assign({}, data.Settings),
          whitelistOption: whitelistOption
        });
      })
    );
  }

  componentDidMount() {
    const { props } = this;
    if (props.otherSettings) {
      this.setState({
        otherSettings: props.otherSettings
      });
      return;
    }
    this.setState({
      siteBehaviorExtrasRendered: true,
      whitelistOption: 0
    });
    this.loadData();
  }

  componentDidUpdate(prevProps) {
    const { props } = this;
    if (props.otherSettings) {
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

  onSettingChange(key, event) {
    let { state, props } = this;
    let otherSettings = Object.assign({}, state.otherSettings);

    otherSettings[key] = typeof event === "object" ? event.target.value : event;

    this.setState({
      otherSettings: otherSettings
    });

    props.dispatch(
      SiteBehaviorActions.otherSettingsClientModified(otherSettings)
    );
  }

  onUpdate(event) {
    if (event) {
      event.preventDefault();
    }
    const { props, state } = this;

    props.dispatch(
      SiteBehaviorActions.updateOtherSettings(
        state.otherSettings,
        () => {},
        () => {
          this.setState({
            errorInSave: true
          });
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
          SiteBehaviorActions.getOtherSettings(data => {
            this.setState({
              otherSettings: Object.assign({}, data.Settings)
            });
          })
        );
        this.setState(
          {
            siteBehaviorExtrasRendered: false
          },
          () => {
            this.setState({
              siteBehaviorExtrasRendered: true
            });
          }
        );
      }
    );
  }

  renderSiteBehaviorExtensions() {
    const SiteBehaviorExtras =
      window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
    if (!SiteBehaviorExtras || SiteBehaviorExtras.length === 0) {
      return;
    }
    return SiteBehaviorExtras.sort(function(a, b) {
      if (a.RenderOrder < b.RenderOrder) return -1;
      if (a.RenderOrder > b.RenderOrder) return 1;
      return 0;
    }).map(data => {
      return data.Component;
    });
  }

  onSaveMoreSettings() {
    const SiteBehaviorExtras =
      window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
    let _errorInSave = false;

    if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
      //First, loop through and check there are no errors.
      SiteBehaviorExtras.forEach(extra => {
        const currentReducer = this.props[extra.ReducerKey];

        if (currentReducer.errors && getError(currentReducer.errors)) {
          extra.SetTriedToSave &&
            this.props.dispatch(extra.SetTriedToSave(true));
          _errorInSave = true;
        } else {
          extra.SetTriedToSave &&
            this.props.dispatch(extra.SetTriedToSave(false));
        }
      });

      //Only go through the save if none of them have an error.
      if (!_errorInSave) {
        SiteBehaviorExtras.forEach(extra => {
          if (typeof extra.SaveMethod === "function") {
            const currentReducer = this.props[extra.ReducerKey];

            //Call the Save Method of each SiteBehaviorExtra.
            this.props.dispatch(
              extra.SaveMethod(
                Object.assign(
                  { formDirty: currentReducer.formDirty },
                  currentReducer.onSavePayload
                ),
                () => {}, //Save Callback
                () => {
                  this.setState({ errorInSave: true });
                } // Error Callback
              )
            );
          }
        });
      } else {
        //Return if there is an error.
        return;
      }
    }

    if (this.props.otherSettingsClientModified) {
      this.onUpdate();
    }

    if (this.state.errorInSave) {
      util.utilities.notifyError(resx.get("SettingsError"));
      this.setState({
        errorInSave: false
      });
    } else {
      util.utilities.notify(resx.get("SettingsUpdateSuccess"));
    }
  }

  getOverallFormDirty() {
    let formDirty = false;
    const SiteBehaviorExtras =
      window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
    if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
      SiteBehaviorExtras.forEach(extra => {
        if (
          this.props[extra.ReducerKey] &&
          this.props[extra.ReducerKey].formDirty
        ) {
          formDirty = true;
        }
      });
    }
    if (this.props.otherSettingsClientModified) {
      formDirty = true;
    }
    return formDirty;
  }

  getWhiteListOptions() {
    return [
      { label: resx.get("Default"), value: 0 },
      { label: resx.get("Custom"), value: 1 },
      { label: resx.get("OnlyImages"), value: 2 }
    ];
  }

  onWhitelistOptionChange(e) {
    var newState = this.state;
    switch (e.value) {
      case 0:
        newState.otherSettings.AllowedExtensionsWhitelist = this.state.otherSettings.HostAllowedExtensionsWhitelists;
        break;
      case 2:
        newState.otherSettings.AllowedExtensionsWhitelist = this.state.otherSettings.ImageExtensionsList;
        break;
    }
    newState.whitelistOption = e.value;
    this.setState(newState);
    this.props.dispatch(
      SiteBehaviorActions.otherSettingsClientModified(newState.otherSettings)
    );
  }

  /* eslint-disable react/no-danger */
  render() {
    const { props, state } = this;
    var htmlEditor = isHost ? (
      <div>
        <div className="sectionTitle">{resx.get("HtmlEditor")}</div>
        <div className="htmlEditorWrapper">
          <div className="htmlEditorWrapper-left">
            <div className="htmlEditorWarning">
              {resx.get("HtmlEditorWarning")}
            </div>
          </div>
          <div className="htmlEditorWrapper-right">
            <Button
              type="secondary"
              onClick={props.openHtmlEditorManager.bind(this)}
            >
              {resx.get("OpenHtmlEditor")}
            </Button>
          </div>
        </div>
      </div>
    ) : null;
    if (state.otherSettings) {
      return (
        <div className={styles.moreSettings}>
          {state.siteBehaviorExtrasRendered &&
            this.renderSiteBehaviorExtensions()}
          {htmlEditor}
          <div className="sectionTitle">{resx.get("WhitelistSettings")}</div>
          <GridSystem numberOfColumns={2}>
            <div key="column-one-left" className="left-column">
              <InputGroup>
                <Label
                  tooltipMessage={resx.get("plWhitelistOption.Help")}
                  label={resx.get("plWhitelistOption")}
                />
                <Dropdown
                  options={this.getWhiteListOptions()}
                  value={state.whitelistOption}
                  onSelect={e => this.onWhitelistOptionChange(e)}
                />
              </InputGroup>
            </div>
            <div key="column-one-right" className="right-column">
              <InputGroup>
                <Label
                  tooltipMessage={resx.get("plAllowedExtensionsWhitelist.Help")}
                  label={resx.get("plAllowedExtensionsWhitelist")}
                />
                <MultiLineInputWithError
                  value={state.otherSettings.AllowedExtensionsWhitelist}
                  onChange={this.onSettingChange.bind(
                    this,
                    "AllowedExtensionsWhitelist"
                  )}
                  enabled={state.whitelistOption == 1}
                />
              </InputGroup>
            </div>
          </GridSystem>
          <div className="buttons-box">
            <Button
              type="secondary"
              disabled={!this.getOverallFormDirty()}
              onClick={this.onCancel.bind(this)}
            >
              {resx.get("Cancel")}
            </Button>
            <Button
              type="primary"
              disabled={!this.getOverallFormDirty()}
              onClick={this.onSaveMoreSettings.bind(this)}
            >
              {resx.get("Save")}
            </Button>
          </div>
        </div>
      );
    } else return <div />;
  }
}

MoreSettingsPanelBody.propTypes = {
  dispatch: PropTypes.func.isRequired,
  portalId: PropTypes.number,
  openHtmlEditorManager: PropTypes.func,
  otherSettings: PropTypes.object,
  otherSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
  return {
    ...state,
    otherSettings: state.siteBehavior.otherSettings,
    otherSettingsClientModified: state.siteBehavior.otherSettingsClientModified
  };
}

export default connect(mapStateToProps)(MoreSettingsPanelBody);
