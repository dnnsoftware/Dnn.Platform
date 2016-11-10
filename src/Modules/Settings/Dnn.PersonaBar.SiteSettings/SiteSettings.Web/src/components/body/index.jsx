import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions
} from "../../actions";
import BasicSettings from "../basicSettings";
import DefaultPagesSettings from "../defaultPagesSettings";
import MessagingSettings from "../messagingSettings";
import ProfileSettings from "../profileSettings";
import SiteAliasSettings from "../siteAliasSettings";
import BasicSearchSettings from "../basicSearchSettings";
import LanguageSettings from "../languageSettings";
import SynonymsGroups from "../synonymsGroups";
import IgnoreWords from "../ignoreWords";
import Tooltip from "dnn-tooltip";
import SocialPanelBody from "dnn-social-panel-body";
import MoreSettings from "../moreSettings";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let isHost = false;

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        isHost = util.settings.isHost;
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    renderSiteBehaviorTab() {
        const {props} = this;
        if (isHost) {
            return <Tabs onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("TabDefaultPages"),
                resx.get("TabMessaging"),
                resx.get("TabUserProfiles"),
                resx.get("TabSiteAliases"),
                resx.get("TabMore")]}
                type="secondary">
                <DefaultPagesSettings portalId={props.portalId} cultureCode={props.cultureCode} />
                <MessagingSettings portalId={props.portalId} cultureCode={props.cultureCode}/>
                <ProfileSettings portalId={props.portalId} cultureCode={props.cultureCode}/>
                <SiteAliasSettings portalId={props.portalId} cultureCode={props.cultureCode}/>
                <MoreSettings portalId={props.portalId} openHtmlEditorManager={props.openHtmlEditorManager.bind(this)} />
            </Tabs>;
        }
        else {
            return <Tabs onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("TabDefaultPages"),
                resx.get("TabUserProfiles")]}
                type="secondary">
                <DefaultPagesSettings portalId={props.portalId} cultureCode={props.cultureCode}/>                
                <ProfileSettings portalId={props.portalId} cultureCode={props.cultureCode}/>
            </Tabs>;
        }
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <SocialPanelBody>
                <Tabs onSelect={this.handleSelect.bind(this)}
                    tabHeaders={[resx.get("TabSiteInfo"),
                    resx.get("TabSiteBehavior"),
                    resx.get("TabLanguage"),
                    resx.get("TabSearch")]}
                    type="primary">
                    <BasicSettings portalId={this.props.portalId} cultureCode={this.props.cultureCode}/>
                    {this.renderSiteBehaviorTab()}
                    <LanguageSettings
                        portalId={this.props.portalId}
                        openLanguageVerifier={this.props.openLanguageVerifier}
                        openLanguagePack={this.props.openLanguagePack}
                        openLocalizedContent={this.props.openLocalizedContent}
                        cultureCode={this.props.cultureCode}
                        />
                    <Tabs onSelect={this.handleSelect.bind(this)}
                        tabHeaders={[resx.get("TabBasicSettings"),
                        resx.get("TabSynonyms"),
                        resx.get("TabIgnoreWords")]}
                        type="secondary">
                        <BasicSearchSettings portalId={this.props.portalId} cultureCode={this.props.cultureCode} />
                        <SynonymsGroups portalId={this.props.portalId} cultureCode={this.props.cultureCode}/>
                        <IgnoreWords portalId={this.props.portalId} cultureCode={this.props.cultureCode}/>
                    </Tabs>
                </Tabs>
            </SocialPanelBody>
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
    openLocalizedContent: PropTypes.func
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index
    };
}

export default connect(mapStateToProps)(Body);