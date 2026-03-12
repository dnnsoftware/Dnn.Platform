import React, { Component } from "react";
import PropTypes from "prop-types";
import { DnnTabs as Tabs, Tooltip, PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import { connect } from "react-redux";
import { pagination as PaginationActions } from "../../actions";
import BasicSettings from "../basicSettings";
import SslSettings from "../sslSettings";
import ApiTokenSettings from "../apiTokenSettings";
import CspSettings from "../cspSettings";
import OtherSettings from "../otherSettings";
import IpFilters from "../ipFilters";
import MemberManagement from "../memberManagement";
import RegistrationSettings from "../registrationSettings";
import SecurityBulletins from "../securityBulletins";
import SuperuserActivity from "../superuserActivity";
import AuditCheck from "../auditCheck";
import ScannerCheck from "../scannerCheck";
import ApiTokens from "../apiTokens";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let isHost = false;
let isAdmin = false;
let canViewBasicLoginSettings = false;
let canViewRegistrationSettings = false;
let canManageApiTokens = false;

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        isHost = util.settings.isHost;
        isAdmin = isHost || util.settings.isAdmin;
        canViewBasicLoginSettings = isHost || isAdmin || util.settings.permissions.BASIC_LOGIN_SETTINGS_VIEW;
        canViewRegistrationSettings = isHost || isAdmin || util.settings.permissions.REGISTRATION_SETTINGS_VIEW;
        canManageApiTokens = isHost || isAdmin || util.settings.permissions.MANAGE_API_TOKENS;
    }

    handleSelect(index) {
        const { props } = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    renderTabs() {
        let securityTabs = [];
        let tabHeaders = [];
        let loginSettingTabHeaders = [];
        let memberAccountsTabHeaders = [];
        let moreTabHeaders = [];
        let moreTabs = [];
        let memberAccountsTabs = [];
        if (canManageApiTokens || canViewBasicLoginSettings || isHost) {
            tabHeaders.push([resx.get("TabAuthenticationSettings")]);
            if (canViewBasicLoginSettings)
                loginSettingTabHeaders.push(resx.get("TabBasicLoginSettings"));
            if (isHost) {
                loginSettingTabHeaders.push(<div style={{ fontSize: "9pt" }}>
                    {resx.get("TabIpFilters")}
                    <Tooltip
                        messages={[resx.get("GlobalSettingsTab")]}
                        type="global"
                        style={{
                            position: "absolute",
                            right: -27,
                            top: 15,
                            textTransform: "none"
                        }} />
                </div>);
            }
            if (canManageApiTokens) {
                loginSettingTabHeaders.push(resx.get("TabApiTokens"));
            }
        }
        if (isHost || canViewRegistrationSettings) {
            tabHeaders.push([resx.get("TabMemberAccounts")]);
            if (isHost) {
                memberAccountsTabHeaders.push(<div style={{ fontSize: "9pt" }}>
                    {resx.get("TabMemberSettings")}
                    <Tooltip
                        messages={[resx.get("GlobalSettingsTab")]}
                        type="global"
                        style={{
                            position: "absolute",
                            right: -27,
                            top: 15,
                            textTransform: "none"
                        }} />
                </div>);
                memberAccountsTabs.push(<MemberManagement key="memberManagement" />);
            }
            if (canViewRegistrationSettings) {
                memberAccountsTabHeaders.push(<div style={{ marginLeft: (isHost ? 35 : 0) }}>
                    <div style={{
                        width: isHost ? 35 : "auto",
                        height: 3,
                        position: "absolute",
                        left: 0,
                        bottom: -3,
                        backgroundColor: "white"
                    }}></div>
                    {resx.get("TabRegistrationSettings")}
                </div>);
                memberAccountsTabs.push(<RegistrationSettings key="registrationSettings" />);
            }
        }
        if (isAdmin) {
            moreTabHeaders.push(resx.get("TabSslSettings"));
            moreTabs.push(<SslSettings key="sslSettings" />);
            moreTabHeaders.push(resx.get("TabApiTokenSettings"));
            moreTabs.push(<ApiTokenSettings key="apiTokenSettings" />);
            moreTabHeaders.push(resx.get("TabCspSettings"));
            moreTabs.push(<CspSettings key="cspSettings" />);
        }
        if (isHost) {
            tabHeaders.push(resx.get("TabSecurityAnalyzer"));
            tabHeaders.push(resx.get("TabSecurityBulletins"));
            moreTabHeaders.push(<div style={{ fontSize: "9pt" }}>
                {resx.get("TabMoreSecuritySettings")}
                <Tooltip
                    messages={[resx.get("GlobalSettingsTab")]}
                    type="global"
                    style={{
                        position: "absolute",
                        right: -27,
                        top: 15,
                        textTransform: "none"
                    }} /></div>);
            moreTabs.push(<OtherSettings key="otherSettings" />);
        }
        if (isAdmin) {
            tabHeaders.push(resx.get("TabMore"));
        }
        let loginSettingsTabs = [];
        if (canViewBasicLoginSettings) {
            loginSettingsTabs.push(<BasicSettings cultureCode={this.props.cultureCode} />);
        }
        if (isHost) {
            loginSettingsTabs.push(<IpFilters />);
        }
        if (canManageApiTokens) {
            loginSettingsTabs.push(<ApiTokens />);
        }
        if (canManageApiTokens || canViewBasicLoginSettings || isHost) {
            securityTabs.push(<Tabs key="loginSettingsTab" onSelect={this.handleSelect.bind(this)}
                tabHeaders={loginSettingTabHeaders}
                type="secondary">
                {loginSettingsTabs}
            </Tabs>);
        }
        if (isHost || canViewRegistrationSettings) {
            securityTabs.push(<Tabs key="memberAccountsTab" onSelect={this.handleSelect.bind(this)}
                tabHeaders={memberAccountsTabHeaders}
                type="secondary">
                {memberAccountsTabs}
            </Tabs>);
        }
        if (isHost) {
            securityTabs.push(<Tabs key="auditChecksTab" onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("TabAuditChecks"), resx.get("TabScannerCheck"), resx.get("TabSuperuserActivity")]}
                type="secondary">
                <AuditCheck />
                <ScannerCheck />
                <SuperuserActivity />
            </Tabs>);
            securityTabs.push(<SecurityBulletins key="securityBulletinsTab" />);
        }
        if (isAdmin) {
            securityTabs.push(<Tabs key="moreTab" onSelect={this.handleSelect.bind(this)}
                tabHeaders={moreTabHeaders}
                type="secondary">
                {moreTabs}
            </Tabs>);
        }
        return <Tabs onSelect={this.handleSelect.bind(this)}
            tabHeaders={tabHeaders}
            type="primary">
            {securityTabs}
        </Tabs>;
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <PersonaBarPageBody>
                {this.renderTabs()}
            </PersonaBarPageBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index
    };
}

export default connect(mapStateToProps)(Body);