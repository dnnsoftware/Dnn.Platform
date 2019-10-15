import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    pagination as PaginationActions
} from "../../actions";
import { DnnTabs, PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import BasicSettings from "../basicSettings";
import DefaultPagesSettings from "../defaultPagesSettings";
import MessagingSettings from "../messagingSettings";
import ProfileSettings from "../profileSettings";
import SiteAliasSettings from "../siteAliasSettings";
import PrivacySettings from "../privacySettings";
import BasicSearchSettings from "../basicSearchSettings";
import LanguageSettings from "../languageSettings";
import SynonymsGroups from "../synonymsGroups";
import IgnoreWords from "../ignoreWords";
import MoreSettings from "../moreSettings";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let isHost = false;
let isAdmin = false;
let canViewSiteInfo = false;
export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        isHost = util.settings.isHost;
        isAdmin = util.settings.isHost || util.settings.isAdmin;
        canViewSiteInfo = isAdmin || util.settings.permissions.SITE_INFO_VIEW;
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    shouldComponentUpdate(nextProps) {
        // Only render if is to show the component.Avoid calling backend when not needed. 
        return nextProps.showing;
    }

    renderSiteBehaviorTab() {
        const {props} = this;
        if (isHost) {
            return <DnnTabs onSelect={this.handleSelect.bind(this) }
                tabHeaders={[resx.get("TabDefaultPages"),
                    resx.get("TabMessaging"),
                    resx.get("TabUserProfiles"),
                    resx.get("TabSiteAliases"),
                    resx.get("TabPrivacy"),
                    resx.get("TabMore")]}
                type="secondary">
                <DefaultPagesSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <MessagingSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <ProfileSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <SiteAliasSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <PrivacySettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <MoreSettings portalId={props.portalId} openHtmlEditorManager={props.openHtmlEditorManager.bind(this) } />
            </DnnTabs>;
        }
        else {
            return <DnnTabs onSelect={this.handleSelect.bind(this) }
                tabHeaders={[resx.get("TabDefaultPages"),
                    resx.get("TabMessaging"),
                    resx.get("TabUserProfiles")]}
                type="secondary">
                <DefaultPagesSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <MessagingSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <ProfileSettings portalId={props.portalId} cultureCode={props.cultureCode} />
            </DnnTabs>;
        }
    }

    getSearchSecondaryTabs() {
        const SearchExtras = window.dnn.SiteSettings && window.dnn.SiteSettings.SearchExtras;

        let searchTabHeaders = [resx.get("TabBasicSettings"), resx.get("TabSynonyms"), resx.get("TabIgnoreWords")];
        let searchTabContent = [
            <BasicSearchSettings portalId={this.props.portalId} cultureCode={this.props.cultureCode} key="first" />,
            <SynonymsGroups portalId={this.props.portalId} cultureCode={this.props.cultureCode} key="second" />,
            <IgnoreWords portalId={this.props.portalId} cultureCode={this.props.cultureCode} key="third" />
        ];

        if (SearchExtras && SearchExtras.length > 0) {
            SearchExtras.sort(function (a, b) {
                if (a.RenderOrder < b.RenderOrder) return -1;
                if (a.RenderOrder > b.RenderOrder) return 1;
                return 0;
            }).forEach((searchExtra, index) => {
                searchTabHeaders.push(searchExtra.TabHeader);
                searchTabContent.push(<div key={"content-" + index}>{searchExtra.Component}</div>);
            });
        }

        return <DnnTabs onSelect={this.handleSelect.bind(this) }
            tabHeaders={searchTabHeaders}
            type="secondary">
            {searchTabContent}
        </DnnTabs>;
    }

    renderBasicSettings() {
        return (<BasicSettings portalId={this.props.portalId} cultureCode={this.props.cultureCode} />);
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        let tabHeaders = [];
        if (canViewSiteInfo)
            tabHeaders.push([resx.get("TabSiteInfo")]);
        if (isAdmin) {
            tabHeaders.push(resx.get("TabSiteBehavior"));
            tabHeaders.push(resx.get("TabLanguage"));
            tabHeaders.push(resx.get("TabSearch"));
        }

        return (
            <PersonaBarPageBody backToLinkProps={{
                text: this.props.referrer && this.props.referrerText,
                onClick: this.props.backToReferrerFunc
            }}>
                <DnnTabs onSelect={this.handleSelect.bind(this) }
                    tabHeaders={tabHeaders}
                    type="primary">
                    {this.props.showing && canViewSiteInfo && this.renderBasicSettings() }
                    {this.props.showing && isAdmin && this.renderSiteBehaviorTab() }
                    {this.props.showing && isAdmin && <LanguageSettings
                        portalId={this.props.portalId}
                        openLanguageVerifier={this.props.openLanguageVerifier}
                        openLanguagePack={this.props.openLanguagePack}
                        openLocalizedContent={this.props.openLocalizedContent}
                        cultureCode={this.props.cultureCode}/>
                    }
                    {this.props.showing && isAdmin && this.getSearchSecondaryTabs() }
                </DnnTabs>
            </PersonaBarPageBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string,
    openHtmlEditorManager: PropTypes.func,
    openLanguageVerifier: PropTypes.func,
    openLanguagePack: PropTypes.func,
    openLocalizedContent: PropTypes.func,
    showing: PropTypes.bool,
    referrer: PropTypes.string,
    referrerText: PropTypes.string,
    backToReferrerFunc: PropTypes.func
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index
    };
}

export default connect(mapStateToProps)(Body);