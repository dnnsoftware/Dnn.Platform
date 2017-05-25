import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions
} from "../../actions";
import BasicSettings from "../basicSettings";
import SslSettings from "../sslSettings";
import OtherSettings from "../otherSettings";
import IpFilters from "../ipFilters";
import Tooltip from "dnn-tooltip";
import MemberManagement from "../memberManagement";
import RegistrationSettings from "../registrationSettings";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import SecurityBulletins from "../securityBulletins";
import SuperuserActivity from "../superuserActivity";
import AuditCheck from "../auditCheck";
import ScannerCheck from "../scannerCheck";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let isHost = false;
let isAdmin = false;
let canViewBasicLoginSettings = false;
let canViewLoginIpFilters = false;
let canViewMemberManagement = false;
let canViewRegistrationSettings = false;
export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        isHost = util.settings.isHost;
        isAdmin = isHost || util.settings.isAdmin;
        canViewBasicLoginSettings = isHost || isAdmin || util.settings.permissions.BASIC_LOGIN_SETTINGS_VIEW;
        canViewLoginIpFilters = isHost || util.settings.permissions.LOGIN_IP_FILTERS_VIEW;
        canViewMemberManagement = isHost || util.settings.permissions.MEMBER_MANAGEMENT_VIEW;
        canViewRegistrationSettings = isHost || isAdmin || util.settings.permissions.REGISTRATION_SETTINGS_VIEW;
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    renderTabs() {
        let tabHeaders = [];
        let loginSettingTabHeaders = [];
        let memberAccountsTabHeaders = [];
        let moreTabHeaders = [];
        if (canViewBasicLoginSettings || canViewLoginIpFilters) {
            tabHeaders.push([resx.get("TabLoginSettings")]);
            if (canViewBasicLoginSettings)
                loginSettingTabHeaders.push(resx.get("TabBasicLoginSettings"));
            if (canViewLoginIpFilters) {
                loginSettingTabHeaders.push(<div style={{ fontSize: "9pt" }}>
                    {resx.get("TabIpFilters") }
                    <Tooltip
                        messages={[resx.get("GlobalSettingsTab")]}
                        type="global"
                        style={{
                            position: "absolute",
                            right: -27,
                            top: 15,
                            textTransform: "none"
                        }}
                        />
                </div>);
            }
        }
        if (canViewMemberManagement || canViewRegistrationSettings) {
            tabHeaders.push([resx.get("TabMemberAccounts")]);
            if (canViewMemberManagement) {
                memberAccountsTabHeaders.push(<div style={{ fontSize: "9pt" }}>
                    {resx.get("TabMemberSettings") }
                    <Tooltip
                        messages={[resx.get("GlobalSettingsTab")]}
                        type="global"
                        style={{
                            position: "absolute",
                            right: -27,
                            top: 15,
                            textTransform: "none"
                        }}
                        />
                </div>);
            }
            if (canViewRegistrationSettings) {
                memberAccountsTabHeaders.push(<div style={{ marginLeft: 35 }}>
                    <div style={{
                        width: 35,
                        height: 3,
                        position: "absolute",
                        left: 0,
                        bottom: -3,
                        backgroundColor: "white"
                    }}></div>
                    {resx.get("TabRegistrationSettings") }
                </div>);
            }
        }
        if (isAdmin) {
            moreTabHeaders.push(resx.get("TabSslSettings"));
        }
        if (isHost) {
            tabHeaders.push(resx.get("TabSecurityAnalyzer"));
            tabHeaders.push(resx.get("TabSecurityBulletins"));
            moreTabHeaders.push(<div style={{ fontSize: "9pt" }}>{resx.get("TabMoreSecuritySettings") } <Tooltip
                messages={[resx.get("GlobalSettingsTab")]}
                type="global"
                style={{
                    position: "absolute",
                    right: -27,
                    top: 15,
                    textTransform: "none"
                }}
                /></div>);
        }
        if (isAdmin) {
            tabHeaders.push(resx.get("TabMore"));
        }

        return <Tabs onSelect={this.handleSelect.bind(this) }
            tabHeaders={tabHeaders}
            type="primary">
            {(canViewBasicLoginSettings || canViewLoginIpFilters) &&
                <Tabs onSelect={this.handleSelect.bind(this) }
                    tabHeaders={loginSettingTabHeaders}
                    type="secondary">
                    {canViewBasicLoginSettings && <BasicSettings cultureCode={this.props.cultureCode} />}
                    {canViewLoginIpFilters && <IpFilters />}
                </Tabs>
            }
            {(canViewMemberManagement || canViewRegistrationSettings) &&
                <Tabs onSelect={this.handleSelect.bind(this) }
                    tabHeaders={memberAccountsTabHeaders}
                    type="secondary">
                    {canViewMemberManagement && <MemberManagement />}
                    {canViewRegistrationSettings && <RegistrationSettings />}
                </Tabs>
            }
            {isHost && <Tabs onSelect={this.handleSelect.bind(this) }
                tabHeaders={[resx.get("TabAuditChecks"), resx.get("TabScannerCheck"), resx.get("TabSuperuserActivity")]}
                type="secondary">
                <AuditCheck />
                <ScannerCheck />
                <SuperuserActivity />
            </Tabs>
            }
            {isHost && <SecurityBulletins />}
            {isAdmin && <Tabs onSelect={this.handleSelect.bind(this) }
                tabHeaders={moreTabHeaders}
                type="secondary">
                <SslSettings />
                {isHost && <OtherSettings />}
            </Tabs>
            }
        </Tabs >;
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <PersonaBarPageBody>
                {this.renderTabs() }
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