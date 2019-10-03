import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../actions";
import Languages from "./languages";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";
import { InputGroup, GridSystem, Dropdown, Flag, RadioButtons, Switch, Tooltip, Label, Button } from "@dnnsoftware/dnn-react-common";

class LanguageSettingsPanelBody extends Component {
    constructor(props) {
        super(props);
        this.state = {
            languageSettings: undefined
        };
    }

    isHost() {
        return util.settings.isHost === true;
    }

    allowContentLocalization() {
        const { props } = this;
        return props.languageSettings !== undefined && props.languageSettings.AllowContentLocalization === true;
    }

    loadData() {
        const {props} = this;
        props.dispatch(LanguagesActions.getLanguageSettings(props.portalId, props.cultureCode, (data) => {
            this.setState({
                languageSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    componentDidMount() {
        this.loadData();
    }

    componentDidUpdate(prevProps) {
        const { props } = this;
        if (props.languageSettings) {
            let portalIdChanged = false;
            let cultureCodeChanged = false;            
            if (props.portalId === undefined || prevProps.portalId === props.portalId) {
                portalIdChanged = false;
            }
            else {
                portalIdChanged = true;
            }
            if (props.cultureCode === undefined || prevProps.cultureCode === props.cultureCode) {
                cultureCodeChanged = false;
            }
            else {
                cultureCodeChanged = true;
            }

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
            }
        }
    }

    onSettingChange(key, event) {
        let {props} = this;
        let languageSettings = Object.assign({}, props.languageSettings);

        if (key === "LanguageDisplayMode") {
            languageSettings[key] = event;
        }
        else if (key === "SiteDefaultLanguage") {
            languageSettings[key] = event.value;
        }
        else {
            languageSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            languageSettings: languageSettings
        });

        props.dispatch(LanguagesActions.languageSettingsClientModified(languageSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props} = this;

        props.dispatch(LanguagesActions.updateLanguageSettings(props.languageSettings, props.languageList, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(LanguagesActions.getLanguageSettings(props.portalId, props.cultureCode, (data) => {
                this.setState({
                    languageSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    getLanguageOptions() {
        const {props} = this;
        let options = [];
        if (props.languages !== undefined) {
            options = props.languages.map((item) => {
                if (props.languageSettings.LanguageDisplayMode === "NATIVE") {
                    return {
                        label: <div title={item.NativeName} style={{ float: "left", display: "flex" }}>
                            <div className="language-flag">
                                <Flag culture={item.Name} title={item.NativeName}/>
                            </div>
                            <div className="language-name">{item.NativeName}</div>
                        </div>, value: item.Name
                    };
                }
                else {
                    return {
                        label: <div title={item.EnglishName} style={{ float: "left", display: "flex" }}>
                            <div className="language-flag">
                                <Flag culture={item.Name} title={item.EnglishName}/>
                            </div>
                            <div className="language-name">{item.EnglishName}</div>
                        </div>, value: item.Name
                    };
                }
            });
        }
        return options;
    }

    getLanguageDisplayModes() {
        const {props} = this;
        let options = [];
        if (props.languageDisplayModes !== undefined) {
            options = props.languageDisplayModes.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    enableLocalizedContent() {
        const {props} = this;
        if (props.languageSettingsClientModified) {
            util.utilities.notifyError(resx.get("SaveOrCancelWarning"));
        }
        else {
            props.openLocalizedContent();
        }
    }

    disableLocalizedContent() {
        const {props} = this;
        if (props.languageSettingsClientModified) {
            util.utilities.notifyError(resx.get("SaveOrCancelWarning"));
        }
        else {
            props.dispatch(LanguagesActions.disableLocalizedContent(props.portalId, () => {
                props.dispatch(LanguagesActions.getLanguageSettings(props.portalId, props.cultureCode));
            }));
        }
    }

    disableLocalizedContentButton() {
        const {props} = this;

        if (this.allowContentLocalization() !== props.languageSettings.AllowContentLocalization) {
            return true;
        }
        else {
            return false;
        }
    }

    getDefaultLanguageDisplay() {
        const {props} = this;
        return (
            <div className="default-language">
                <div className="language-container">
                    <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={props.languageSettings.SystemDefaultLanguageIcon} alt={props.languageSettings.SystemDefaultLanguage} />
                        </div>
                        <div className="language-name">{props.languageSettings.SystemDefaultLanguage}</div>
                    </div>
                </div>
            </div>
        );
    }

    /* eslint-disable react/no-danger test */
    render() {
        const {props} = this;
        if (props.languageSettings) {
            const columnOne = <div className="left-column" key="language-settings-left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("systemDefaultLabel.Help")}
                        label={resx.get("systemDefaultLabel")}
                    />
                    {this.getDefaultLanguageDisplay()}
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("siteDefaultLabel.Help")}
                        label={resx.get("siteDefaultLabel")}
                    />
                    <Dropdown
                        options={this.getLanguageOptions()}
                        value={props.languageSettings.SiteDefaultLanguage}
                        onSelect={this.onSettingChange.bind(this, "SiteDefaultLanguage")}
                        enabled={!props.languageSettings.ContentLocalizationEnabled}
                        getLabelText={(label) => label.props.title}
                    />
                    <RadioButtons
                        onChange={this.onSettingChange.bind(this, "LanguageDisplayMode")}
                        options={this.getLanguageDisplayModes()}
                        buttonGroup="languageDisplayMode"
                        value={props.languageSettings.LanguageDisplayMode}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div className="right-column" key="language-settings-right-column">
                <InputGroup>
                    <div className="languageSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("plUrl.Help")}
                            label={resx.get("plUrl")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={props.languageSettings.EnableUrlLanguage}
                            onChange={this.onSettingChange.bind(this, "EnableUrlLanguage")}
                            readOnly={props.languageSettings.ContentLocalizationEnabled}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="languageSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("detectBrowserLable.Help")}
                            label={resx.get("detectBrowserLable")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={props.languageSettings.EnableBrowserLanguage}
                            onChange={this.onSettingChange.bind(this, "EnableBrowserLanguage")}
                        />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="languageSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("allowUserCulture.Help")}
                            label={resx.get("allowUserCulture")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={props.languageSettings.AllowUserUICulture}
                            onChange={this.onSettingChange.bind(this, "AllowUserUICulture")}
                        />
                    </div>
                </InputGroup>
                {this.isHost() &&
                    <InputGroup>
                        <div className="languageSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableContentLocalization.Help")}
                                label={resx.get("plEnableContentLocalization")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }}
                                    />}
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={props.languageSettings.AllowContentLocalization}
                                onChange={this.onSettingChange.bind(this, "AllowContentLocalization")}
                            />
                        </div>
                    </InputGroup>
                }
                <div className={"collapsible-button" + (this.allowContentLocalization() || props.languageSettings.ContentLocalizationEnabled ? " open" : "")}>
                    {!props.languageSettings.ContentLocalizationEnabled && <Button
                        type="secondary"
                        onClick={this.enableLocalizedContent.bind(this)}
                        disabled={this.disableLocalizedContentButton()}>
                        {resx.get("EnableLocalizedContent")}
                    </Button>}
                    {props.languageSettings.ContentLocalizationEnabled && <Button
                        type="secondary"
                        onClick={this.disableLocalizedContent.bind(this)}
                        disabled={this.disableLocalizedContentButton()}>
                        {resx.get("DisableLocalizedContent")}
                    </Button>}
                </div>
            </div>;

            return (
                <div className={styles.languageSettings}>
                    <Languages
                        portalId={this.props.portalId}
                        languageDisplayMode={props.languageSettings.LanguageDisplayMode}
                        contentLocalizationEnabled={props.languageSettings.ContentLocalizationEnabled}
                    />
                    <div className="sectionTitle">{resx.get("LanguageSettings")}</div>
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className={this.isHost() ? "buttons-box-alter" : "buttons-box"}>
                        <Button
                            disabled={!this.props.languageSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        {this.isHost() &&
                            <Button
                                type="secondary"
                                onClick={props.openLanguageVerifier}>
                                {resx.get("VerifyLanguageResources")}
                            </Button>
                        }
                        {this.isHost() &&
                            <Button
                                type="secondary"
                                onClick={props.openLanguagePack}>
                                {resx.get("CreateLanguagePack")}
                            </Button>
                        }
                        <Button
                            disabled={!this.props.languageSettingsClientModified}
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

LanguageSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    languageSettings: PropTypes.object,
    languages: PropTypes.array,
    languageDisplayModes: PropTypes.array,
    languageSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string,
    openLanguageVerifier: PropTypes.func,
    openLanguagePack: PropTypes.func,
    openLocalizedContent: PropTypes.func,
    languageList: PropTypes.array
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        languageSettings: state.languages.languageSettings,
        languages: state.languages.languages,
        languageDisplayModes: state.languages.languageDisplayModes,
        languageSettingsClientModified: state.languages.languageSettingsClientModified,
        languageList: state.languages.languageList,
        portalId: state.siteInfo ? state.siteInfo.portalId : undefined
    };
}

export default connect(mapStateToProps)(LanguageSettingsPanelBody);