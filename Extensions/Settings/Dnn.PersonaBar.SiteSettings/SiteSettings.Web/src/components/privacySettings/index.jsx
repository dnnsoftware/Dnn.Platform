import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { siteBehavior as SiteBehaviorActions } from "../../actions";
import InputGroup from "dnn-input-group";
import Switch from "dnn-switch";
import Grid from "dnn-grid-system";
import Tooltip from "dnn-tooltip";
import Label from "dnn-label";
import Button from "dnn-button";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
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
        if (props.privacySettings) {
            let portalIdChanged = false;
            let cultureCodeChanged = false;

            if (
                props.portalId === undefined ||
                props.privacySettings.PortalId === props.portalId
            ) {
                portalIdChanged = false;
            } else {
                portalIdChanged = true;
            }

            if (
                props.cultureCode === undefined ||
                props.privacySettings.CultureCode === props.cultureCode
            ) {
                cultureCodeChanged = false;
            } else {
                cultureCodeChanged = true;
            }

            if (portalIdChanged || cultureCodeChanged) {
                return true;
            } else return false;
        } else {
            return true;
        }
    }

    componentDidMount() {
        const { props } = this;
        if (!this.loadData()) {
            this.setState({
                privacySettings: props.privacySettings
            });
            return;
        }
        props.dispatch(
            SiteBehaviorActions.getPrivacySettings(props.portalId, data => {
                this.setState({
                    privacySettings: Object.assign({}, data.Settings)
                });
            })
        );
    }

    componentDidUpdate(props) {
        this.setState({
            privacySettings: Object.assign({}, props.privacySettings)
        });
    }

    onSettingChange(key, event) {
        let { state, props } = this;
        let privacySettings = Object.assign({}, state.privacySettings);

        if (key === "ThrottlingInterval" || key === "RecipientLimit") {
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
        const { state } = this;
        const columnOneLeft = state.privacySettings ? (
            <div className="left-column">
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
            <div className="left-column" />
        );
        const columnOneRight = (
            <div className="right-column">
                {state.privacySettings && (
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
                            onChange={this.onSettingChange.bind(
                                this,
                                "DnnImprovementProgram"
                            )}
                        />
                    </InputGroup>
                )}
            </div>
        );
        const columnTwoLeft = (
            <div className="left-column">
                {state.privacySettings && (
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
                )}
            </div>
        );
        const columnTwoRight = (
            <div className="right-column">
                {state.privacySettings && (
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
                )}
            </div>
        );

        return (
            <div className={styles.privacySettings}>
                <div className="sectionTitle">
                    {resx.get("PrivacyCommunicationSettings")}
                </div>
                <Grid numberOfColumns={2}>{[columnOneLeft, columnOneRight]}</Grid>
                <div className="sectionTitle">
                    {resx.get("PrivacyCookieConsentSettings")}
                </div>
                <Grid numberOfColumns={2}>{[columnTwoLeft, columnTwoRight]}</Grid>
                <div className="buttons-box">
                    <Button
                        disabled={!this.props.privacySettingsClientModified}
                        type="secondary"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!this.props.privacySettingsClientModified}
                        type="primary"
                        onClick={this.onUpdate.bind(this)}>
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
        privacySettingsClientModified: state.siteBehavior.privacySettingsClientModified
    };
}

export default connect(mapStateToProps)(PrivacySettingsPanelBody);
