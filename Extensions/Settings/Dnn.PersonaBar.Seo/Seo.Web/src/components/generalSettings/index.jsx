import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    seo as SeoActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import Grid from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import RadioButtons from "dnn-radio-buttons";
import Switch from "dnn-switch";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class GeneralSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            generalSettings: undefined,
            triedToSubmit: false
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.generalSettings) {
            this.setState({
                generalSettings: props.generalSettings
            });
            return;
        }
        props.dispatch(SeoActions.getGeneralSettings((data) => {
            this.setState({
                generalSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    UNSAFE_componentWillReceiveProps(props) {
        this.setState({
            generalSettings: Object.assign({}, props.generalSettings),
            triedToSubmit: false
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let generalSettings = Object.assign({}, state.generalSettings);

        if (key === "ReplaceSpaceWith") {
            generalSettings[key] = event.value;
        }
        else {
            generalSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            generalSettings: generalSettings,
            triedToSubmit: false
        });

        props.dispatch(SeoActions.generalSettingsClientModified(generalSettings));
    }

    keyValuePairsToOptions(keyValuePairs) {
        let options = [];
        if (keyValuePairs !== undefined) {
            options = keyValuePairs.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        props.dispatch(SeoActions.updateGeneralSettings(state.generalSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SeoActions.getGeneralSettings((data) => {
                this.setState({
                    generalSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.generalSettings) {
            const columnOne = <div className="left-column">
                <div className="columnTitle">{resx.get("UrlRewriter")}</div>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ReplacementCharacter.Help")}
                        label={resx.get("ReplacementCharacter")} />
                    <Dropdown
                        options={this.keyValuePairsToOptions(props.replacementCharacterList)}
                        value={state.generalSettings.ReplaceSpaceWith}
                        onSelect={this.onSettingChange.bind(this, "ReplaceSpaceWith")} />
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("enableSystemGeneratedUrlsLabel.Help")}
                            label={resx.get("enableSystemGeneratedUrlsLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.EnableSystemGeneratedUrls}
                            onChange={this.onSettingChange.bind(this, "EnableSystemGeneratedUrls")} />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("enableLowerCaseLabel.Help")}
                            label={resx.get("enableLowerCaseLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.ForceLowerCase}
                            onChange={this.onSettingChange.bind(this, "ForceLowerCase")} />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("autoAsciiConvertLabel.Help")}
                            label={resx.get("autoAsciiConvertLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.AutoAsciiConvert}
                            onChange={this.onSettingChange.bind(this, "AutoAsciiConvert")} />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("setDefaultSiteLanguageLabel.Help")}
                            label={resx.get("setDefaultSiteLanguageLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.ForcePortalDefaultLanguage}
                            onChange={this.onSettingChange.bind(this, "ForcePortalDefaultLanguage")} />
                    </div>
                </InputGroup>
            </div>;
            const columnTwo = <div className="right-column">
                <div className="columnTitle">{resx.get("UrlRedirects")}</div>
                <InputGroup>
                    <div className="generalSettings-row-options">
                        <Label
                            tooltipMessage={resx.get("plDeletedPages.Help")}
                            label={resx.get("plDeletedPages")} />
                        <RadioButtons
                            onChange={this.onSettingChange.bind(this, "DeletedTabHandlingType")}
                            options={this.keyValuePairsToOptions(props.deletedPageHandlingTypes)}
                            buttonGroup="deletedTabHandlingType"
                            value={state.generalSettings.DeletedTabHandlingType} />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("enable301RedirectsLabel.Help")}
                            label={resx.get("enable301RedirectsLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.RedirectUnfriendly}
                            onChange={this.onSettingChange.bind(this, "RedirectUnfriendly")} />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="generalSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("redirectOnWrongCaseLabel.Help")}
                            label={resx.get("redirectOnWrongCaseLabel")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.generalSettings.RedirectWrongCase}
                            onChange={this.onSettingChange.bind(this, "RedirectWrongCase")} />
                    </div>
                </InputGroup>
            </div>;

            return (
                <div className={styles.generalSettings}>
                    <Grid numberOfColumns={2}>{[columnOne, columnTwo]}</Grid>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.clientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.clientModified}
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

GeneralSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    generalSettings: PropTypes.object,
    deletedPageHandlingTypes: PropTypes.array,
    replacementCharacterList: PropTypes.array,
    clientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        generalSettings: state.seo.generalSettings,
        deletedPageHandlingTypes: state.seo.deletedPageHandlingTypes,
        replacementCharacterList: state.seo.replacementCharacterList,
        clientModified: state.seo.clientModified
    };
}

export default connect(mapStateToProps)(GeneralSettingsPanelBody);