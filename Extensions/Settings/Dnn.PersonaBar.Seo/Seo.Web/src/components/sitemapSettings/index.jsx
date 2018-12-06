import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    seo as SeoActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import Switch from "dnn-switch";
import Button from "dnn-button";
import ProviderRow from "./providerRow";
import ProviderEditor from "./providerEditor";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let daysToCacheOptions = [];
let priorityOptions = [];
let tableFields = [];

const re = /[^].html$/;

class SitemapSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            sitemapSettings: undefined,
            searchEngine: "Google",
            verification: "",
            triedToSubmit: false,
            triedToCreate: false,
            openId: "",
            error: {
                verificationValidity: true
            }
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.sitemapSettings) {
            this.setState({
                sitemapSettings: props.sitemapSettings
            });
            return;
        }

        daysToCacheOptions = [];
        daysToCacheOptions.push({ "value": 0, "label": resx.get("DisableCaching") });
        daysToCacheOptions.push({ "value": 1, "label": resx.get("1Day") });
        daysToCacheOptions.push({ "value": 2, "label": resx.get("2Days") });
        daysToCacheOptions.push({ "value": 3, "label": resx.get("3Days") });
        daysToCacheOptions.push({ "value": 4, "label": resx.get("4Days") });
        daysToCacheOptions.push({ "value": 5, "label": resx.get("5Days") });
        daysToCacheOptions.push({ "value": 6, "label": resx.get("6Days") });
        daysToCacheOptions.push({ "value": 7, "label": resx.get("7Days") });

        priorityOptions = [];
        priorityOptions.push({ "value": 1, "label": "1" });
        priorityOptions.push({ "value": 0.9, "label": "0.9" });
        priorityOptions.push({ "value": 0.8, "label": "0.8" });
        priorityOptions.push({ "value": 0.7, "label": "0.7" });
        priorityOptions.push({ "value": 0.6, "label": "0.6" });
        priorityOptions.push({ "value": 0.5, "label": "0.5" });
        priorityOptions.push({ "value": 0.4, "label": "0.4" });
        priorityOptions.push({ "value": 0.3, "label": "0.3" });
        priorityOptions.push({ "value": 0.2, "label": "0.2" });
        priorityOptions.push({ "value": 0.1, "label": "0.1" });
        priorityOptions.push({ "value": 0, "label": "0" });

        tableFields = [];
        tableFields.push({ "name": resx.get("Name.Header"), "id": "Name" });
        tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });
        tableFields.push({ "name": resx.get("Priority.Header"), "id": "Priority" });

        props.dispatch(SeoActions.getSitemapSettings((data) => {
            this.setState({
                sitemapSettings: Object.assign({}, data.Settings)
            });
        }));

        props.dispatch(SeoActions.getSitemapProviders((data) => {
            this.setState({
                sitemapProviders: Object.assign({}, data.Providers)
            });
        }));
    }

    UNSAFE_componentWillReceiveProps(props) {
        this.setState({
            sitemapSettings: Object.assign({}, props.sitemapSettings),
            triedToSubmit: false,
            triedToCreate: false
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let sitemapSettings = Object.assign({}, state.sitemapSettings);

        if (key === "SitemapExcludePriority" || key === "SitemapCacheDays" || key === "SitemapMinPriority") {
            sitemapSettings[key] = event.value;
        }
        else {
            sitemapSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            sitemapSettings: sitemapSettings,
            triedToSubmit: false
        });

        props.dispatch(SeoActions.sitemapSettingsClientModified(sitemapSettings));
    }

    searchEngineListToOptions(list) {
        let options = [];
        if (list !== undefined) {
            options = list.map((item) => {
                return { label: item.Key, value: item.Key };
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

        props.dispatch(SeoActions.updateSitemapSettings(state.sitemapSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SeoActions.getSitemapSettings((data) => {
                this.setState({
                    sitemapSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    onClearCache() {
        const {props} = this;
        props.dispatch(SeoActions.clearCache());
    }

    onUpdateProvider(settings) {
        const {props} = this;

        props.dispatch(SeoActions.updateSitemapProvider(settings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
            props.dispatch(SeoActions.getSitemapProviders((data) => {
                this.setState({
                    sitemapProviders: Object.assign({}, data.Providers)
                });
            }));
            this.collapse();
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }
    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }
    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        } else {
            this.collapse();
        }
    }

    renderProvidersHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "provider-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}</span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    renderedProviders() {
        if (this.props.providers) {
            return this.props.providers.map((item, index) => {
                return (
                    <ProviderRow
                        name={item.Name}
                        enabled={item.Enabled}
                        priority={item.Priority}
                        overridePriority={item.OverridePriority}
                        index={index}
                        key={"provider-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}>
                        <ProviderEditor
                            settings={item}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateProvider.bind(this)}
                            openId={this.state.openId} />
                    </ProviderRow>
                );
            });
        }
    }

    onSearchEngineChange(event) {
        this.setState({
            searchEngine: event.value
        });
    }

    onSearchEngineSubmit() {
        let {state, props} = this;
        let url = props.searchEngineUrls.filter((item) => item.Key === state.searchEngine)[0].Value;
        window.open(url, "_blank");
    }

    onVerificationChange(event) {
        let {state} = this;
        let verification = typeof (event) === "object" ? event.target.value : event;
        if (verification === "" || !re.test(verification)) {
            state.error["verificationValidity"] = true;
        }
        else if (verification !== "" && re.test(verification)) {
            state.error["verificationValidity"] = false;
        }

        this.setState({
            verification: verification,
            error: state.error,
            triedToCreate: false
        });
    }

    onCreateVerification() {
        let {state, props} = this;
        this.setState({
            triedToCreate: true
        });

        if (state.error.verificationValidity) {
            return;
        }

        props.dispatch(SeoActions.createVerification(state.verification));
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.sitemapSettings) {
            const columnOne = <div className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("sitemapUrlLabel.Help")}
                        label={resx.get("sitemapUrlLabel")} />
                    <div className="sitemapUrl">
                        <a
                            href={state.sitemapSettings.SitemapUrl}
                            target="_blank"
                            rel="noopener noreferrer">
                            {state.sitemapSettings.SitemapUrl}
                        </a>
                    </div>
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("lblExcludePriority.Help")}
                        label={resx.get("lblExcludePriority")} />
                    <Dropdown
                        options={priorityOptions}
                        value={state.sitemapSettings.SitemapExcludePriority}
                        onSelect={this.onSettingChange.bind(this, "SitemapExcludePriority")} />
                </InputGroup>
                <InputGroup>
                    <div className="sitemapSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("lblIncludeHidden.Help")}
                            label={resx.get("lblIncludeHidden")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.sitemapSettings.SitemapIncludeHidden}
                            onChange={this.onSettingChange.bind(this, "SitemapIncludeHidden")} />
                    </div>
                </InputGroup>
            </div>;
            const columnTwo = <div className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("lblCache.Help")}
                        label={resx.get("lblCache")} />
                    <div className="daysToCache">
                        <Dropdown
                            options={daysToCacheOptions}
                            value={state.sitemapSettings.SitemapCacheDays}
                            onSelect={this.onSettingChange.bind(this, "SitemapCacheDays")} />
                        <Button
                            className="clearCacheBtn"
                            type="secondary"
                            onClick={this.onClearCache.bind(this)}>
                            {resx.get("lnkResetCache")}
                        </Button>
                    </div>
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("lblMinPagePriority.Help")}
                        label={resx.get("lblMinPagePriority")} />
                    <Dropdown
                        options={priorityOptions}
                        value={state.sitemapSettings.SitemapMinPriority}
                        onSelect={this.onSettingChange.bind(this, "SitemapMinPriority")} />
                </InputGroup>
                <InputGroup>
                    <div className="sitemapSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("lblLevelPriority.Help")}
                            label={resx.get("lblLevelPriority")} />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.sitemapSettings.SitemapLevelMode}
                            onChange={this.onSettingChange.bind(this, "SitemapLevelMode")} />
                    </div>
                </InputGroup>
            </div>;

            const columnThree = <div className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("lblSearchEngine.Help")}
                        label={resx.get("lblSearchEngine")} />
                    <div className="searchEngineSubmit">
                        <Dropdown
                            options={this.searchEngineListToOptions(props.searchEngineUrls)}
                            value={state.searchEngine}
                            onSelect={this.onSearchEngineChange.bind(this)} />
                        <Button
                            className="searchEngineSubmitBtn"
                            type="secondary"
                            onClick={this.onSearchEngineSubmit.bind(this)}>
                            {resx.get("Submit")}
                        </Button>
                    </div>
                </InputGroup>
            </div>;

            const columnFour = <div className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("lblVerification.Help")}
                        label={resx.get("lblVerification")} />
                    <div className="createVerification">
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={this.state.error.verificationValidity && this.state.triedToCreate}
                            errorMessage={resx.get("VerificationValidity.ErrorMessage")}
                            value={state.verification}
                            onChange={this.onVerificationChange.bind(this)} />
                        <Button
                            className="createVerificationBtn"
                            type="secondary"
                            onClick={this.onCreateVerification.bind(this)}>
                            {resx.get("Create")}
                        </Button>
                    </div>
                </InputGroup>
            </div>;

            return (
                <div className={styles.sitemapSettings}>
                    <div className="columnTitle">{resx.get("SitemapSettings")}</div>
                    <Grid numberOfColumns={2}>{[columnOne, columnTwo]}</Grid>
                    <div className="columnTitle2">{resx.get("SitemapProviders")}</div>
                    <div className="provider-items-grid">
                        {this.renderProvidersHeader()}
                        {this.renderedProviders()}
                    </div>
                    <div className="columnTitle3">{resx.get("SiteSubmission")}</div>
                    <Grid numberOfColumns={2}>{[columnThree, columnFour]}</Grid>
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

SitemapSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    sitemapSettings: PropTypes.object,
    searchEngineUrls: PropTypes.array,
    clientModified: PropTypes.bool,
    providers: PropTypes.array
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        sitemapSettings: state.seo.sitemapSettings,
        searchEngineUrls: state.seo.searchEngineUrls,
        clientModified: state.seo.clientModified,
        providers: state.seo.sitemapProviders
    };
}

export default connect(mapStateToProps)(SitemapSettingsPanelBody);