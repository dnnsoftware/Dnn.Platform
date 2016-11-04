import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    seo as SeoActions
} from "../../actions";
import GeneralSettings from "../generalSettings";
import RegexSettings from "../regexSettings";
import SitemapSettings from "../sitemapSettings";
import TestUrl from "../testUrl";
import Tooltip from "dnn-tooltip";
import SocialPanelBody from "dnn-social-panel-body";
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

    renderTabs() {
        const {props} = this;
        if (isHost) {
            return <Tabs onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("URLManagementTab"),
                resx.get("SitemapSettingsTab")]}
                type="primary">
                <Tabs onSelect={this.handleSelect.bind(this)}
                    tabHeaders={[resx.get("GeneralSettingsTab"), <div style={{ fontSize: "9pt" }}>{resx.get("ExpressionsTab")} <Tooltip
                        messages={[resx.get("GlobalSettingsTab")]}
                        type="global"
                        style={{ float: "right", height: "20", position: "static", margin: "-5px 0 0 5px" }}
                        /></div>, resx.get("TestURLTab")]}
                    type="secondary">
                    <GeneralSettings />
                    <RegexSettings />
                    <TestUrl />
                </Tabs>
                <SitemapSettings />
            </Tabs>;
        }
        else {
            return <Tabs onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("URLManagementTab"),
                resx.get("SitemapSettingsTab")]}
                type="primary">
                <Tabs onSelect={this.handleSelect.bind(this)}
                    tabHeaders={[resx.get("GeneralSettingsTab"), resx.get("TestURLTab")]}
                    type="secondary">
                    <GeneralSettings />
                    <TestUrl />
                </Tabs>
                <SitemapSettings />
            </Tabs>;
        }
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <SocialPanelBody>
                {this.renderTabs()}
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