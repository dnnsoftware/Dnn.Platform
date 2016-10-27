import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    siteSettings as SiteISettingsActions
} from "../../actions";
import BasicSettings from "../basicSettings";
import DefaultPagesSettings from "../defaultPagesSettings";
import MessagingSettings from "../messagingSettings";
import ProfileSettings from "../profileSettings";
import SiteAliasSettings from "../siteAliasSettings";
import Tooltip from "dnn-tooltip";
import SocialPanelBody from "dnn-social-panel-body";
import "./style.less";
import resx from "../../resources";

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <SocialPanelBody>
                <Tabs onSelect={this.handleSelect.bind(this) }
                    tabHeaders={[resx.get("TabSiteInfo"),
                        resx.get("TabSiteBehavior"),
                        resx.get("TabLanguage"),
                        resx.get("TabSearch")]}
                    type="primary">
                    <BasicSettings/>
                    <Tabs onSelect={this.handleSelect.bind(this) }
                        tabHeaders={[resx.get("TabDefaultPages"), 
                        resx.get("TabMessaging"),
                        resx.get("TabUserProfiles"),
                        resx.get("TabSiteAliases"),
                        resx.get("TabMore")]}
                        type="secondary">
                        <DefaultPagesSettings/>
                        <MessagingSettings/>
                        <ProfileSettings/>
                        <SiteAliasSettings/>
                        <div/>
                    </Tabs>
                    <div/>
                    <Tabs onSelect={this.handleSelect.bind(this) }
                        tabHeaders={[resx.get("TabBasicSettings"), 
                        resx.get("TabSynonyms"), 
                        resx.get("TabIgnoreWords"), 
                        resx.get("TabCrawling"), 
                        resx.get("TabFileExtensions")]}
                        type="secondary">
                        <div/>
                        <div/>
                        <div/>
                        <div/>
                        <div/>
                    </Tabs>
                </Tabs>
            </SocialPanelBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index
    };
}

export default connect(mapStateToProps)(Body);