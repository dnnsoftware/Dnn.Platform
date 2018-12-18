import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import { InputGroup, MultiLineInputWithError, PagePicker, GridSystem, Label, Button } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let isHost = false;

class DefaultPagesSettingsPanelBody extends Component {
    constructor() {
        super();
        this._mounted = false;
        this.state = {
            defaultPagesSettings: undefined
        };
        isHost = util.settings.isHost;
    }

    componentDidMount() {
        this._mounted = true;
        this.loadData();
    }

    componentDidUpdate(prevProps) {
        let { props} = this;

        const portalIdChanged = !prevProps.portalId && prevProps.portalId !== props.portalId;
        const cultureCodeChanged = !prevProps.cultureCode && prevProps.cultureCode !== props.cultureCode;

        if(portalIdChanged || cultureCodeChanged) {
            this.loadData();
        }
    }

    componentWillUnmount() {
        this._mounted = false;
    }

    loadData() {
        const { props } = this;
        props.dispatch(SiteBehaviorActions.getDefaultPagesSettings(props.portalId, props.cultureCode, (data) => {
            if(this._mounted) {
                this.setState({
                    defaultPagesSettings: Object.assign({}, data.Settings)
                });
            }
        }));
    }

    onSettingChange(key, event, value) {
        let {state, props} = this;

        let defaultPagesSettings = Object.assign({}, state.defaultPagesSettings);

        if (key === "SplashTabId" || key === "HomeTabId" || key === "LoginTabId" || key === "RegisterTabId" ||
            key === "UserTabId" || key === "SearchTabId" || key === "Custom404TabId" || key === "Custom500TabId" ||
            key === "TermsTabId" || key === "PrivacyTabId") {
            if (defaultPagesSettings[key] !== parseInt(event)) {
                defaultPagesSettings[key] = event;
                defaultPagesSettings[key.replace("Id", "Name")] = value;
            }
            else {
                return;
            }
        }
        else {
            defaultPagesSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            defaultPagesSettings: defaultPagesSettings
        });

        props.dispatch(SiteBehaviorActions.defaultPagesSettingsClientModified(defaultPagesSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteBehaviorActions.updateDefaultPagesSettings(state.defaultPagesSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getDefaultPagesSettings(props.portalId, props.cultureCode, (data) => {
                let defaultPagesSettings = Object.assign({}, data.Settings);
                this.setState({
                    defaultPagesSettings
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        const TabParameters = {
            portalId: props.portalId !== undefined ? props.portalId : -2,
            cultureCode: props.cultureCode || "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            roles: "",
            sortOrder: 0
        };
        let TabParameters_1 = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false });
        let TabParameters_2 = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: true });
        let TabParameters_Login = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false, validateTab: "Account Login" });
        let TabParameters_Search = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false, validateTab: "Search Results" });
        if (state.defaultPagesSettings) {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plSplashTabId.Help")}
                        label={resx.get("plSplashTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 6 }}
                        selectedTabId={state.defaultPagesSettings.SplashTabId ? state.defaultPagesSettings.SplashTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "SplashTabId")}
                        defaultLabel={state.defaultPagesSettings.SplashTabName ? state.defaultPagesSettings.SplashTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plHomeTabId.Help")}
                        label={resx.get("plHomeTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 5 }}
                        selectedTabId={state.defaultPagesSettings.HomeTabId ? state.defaultPagesSettings.HomeTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "HomeTabId")}
                        defaultLabel={state.defaultPagesSettings.HomeTabName ? state.defaultPagesSettings.HomeTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plLoginTabId.Help")}
                        label={resx.get("plLoginTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 4 }}
                        selectedTabId={state.defaultPagesSettings.LoginTabId ? state.defaultPagesSettings.LoginTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "LoginTabId")}
                        defaultLabel={state.defaultPagesSettings.LoginTabName ? state.defaultPagesSettings.LoginTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_Login}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plRegisterTabId.Help")}
                        label={resx.get("plRegisterTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 3 }}
                        selectedTabId={state.defaultPagesSettings.RegisterTabId ? state.defaultPagesSettings.RegisterTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "RegisterTabId")}
                        defaultLabel={state.defaultPagesSettings.RegisterTabName ? state.defaultPagesSettings.RegisterTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plUserTabId.Help")}
                        label={resx.get("plUserTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 2 }}
                        selectedTabId={state.defaultPagesSettings.UserTabId ? state.defaultPagesSettings.UserTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "UserTabId")}
                        defaultLabel={state.defaultPagesSettings.UserTabName ? state.defaultPagesSettings.UserTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plSearchTabId.Help")}
                        label={resx.get("plSearchTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 6 }}
                        selectedTabId={state.defaultPagesSettings.SearchTabId ? state.defaultPagesSettings.SearchTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "SearchTabId")}
                        defaultLabel={state.defaultPagesSettings.SearchTabName ? state.defaultPagesSettings.SearchTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_Search}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("pl404TabId.Help")}
                        label={resx.get("pl404TabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 5 }}
                        selectedTabId={state.defaultPagesSettings.Custom404TabId ? state.defaultPagesSettings.Custom404TabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "Custom404TabId")}
                        defaultLabel={state.defaultPagesSettings.Custom404TabName ? state.defaultPagesSettings.Custom404TabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("pl500TabId.Help")}
                        label={resx.get("pl500TabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 4 }}
                        selectedTabId={state.defaultPagesSettings.Custom500TabId ? state.defaultPagesSettings.Custom500TabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "Custom500TabId")}
                        defaultLabel={state.defaultPagesSettings.Custom500TabName ? state.defaultPagesSettings.Custom500TabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plTermsTabId.Help")}
                        label={resx.get("plTermsTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 3 }}
                        selectedTabId={state.defaultPagesSettings.TermsTabId ? state.defaultPagesSettings.TermsTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "TermsTabId")}
                        defaultLabel={state.defaultPagesSettings.TermsTabName ? state.defaultPagesSettings.TermsTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPrivacyTabId.Help")}
                        label={resx.get("plPrivacyTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 2 }}
                        selectedTabId={state.defaultPagesSettings.PrivacyTabId ? state.defaultPagesSettings.PrivacyTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "PrivacyTabId")}
                        defaultLabel={state.defaultPagesSettings.PrivacyTabName ? state.defaultPagesSettings.PrivacyTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
            </div>;

            return (
                <div className={styles.defaultPagesSettings}>
                    <GridSystem numberOfColumns={2}>
                        {[columnOne, columnTwo]}
                    </GridSystem>
                    {isHost &&
                        <div className="sectionTitle">{resx.get("PageOutputSettings")}</div>}
                    {isHost &&
                        <InputGroup style={{ paddingTop: "10px" }}>
                            <Label
                                tooltipMessage={resx.get("plPageHeadText.Help")}
                                label={resx.get("plPageHeadText")}
                            />
                            <MultiLineInputWithError
                                value={state.defaultPagesSettings.PageHeadText}
                                onChange={this.onSettingChange.bind(this, "PageHeadText")}
                            />
                        </InputGroup>
                    }

                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.defaultPagesSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.defaultPagesSettingsClientModified}
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

DefaultPagesSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    defaultPagesSettings: PropTypes.object,
    defaultPagesSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        defaultPagesSettings: state.siteBehavior.defaultPagesSettings,
        defaultPagesSettingsClientModified: state.siteBehavior.defaultPagesSettingsClientModified,
        portalId: state.siteInfo.settings ? state.siteInfo.settings.PortalId : undefined,
    };
}

export default connect(mapStateToProps)(DefaultPagesSettingsPanelBody);