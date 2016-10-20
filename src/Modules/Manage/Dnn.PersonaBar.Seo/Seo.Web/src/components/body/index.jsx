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
                    tabHeaders={[resx.get("URLManagementTab"),                        
                        resx.get("SitemapSettingsTab")]}
                    type="primary">
                    <Tabs onSelect={this.handleSelect.bind(this) }
                        tabHeaders={[resx.get("GeneralSettingsTab"), resx.get("ExpressionsTab"), resx.get("TestURLTab")]}
                        type="secondary">
                        <GeneralSettings/>
                        <RegexSettings/>
                        <TestUrl/>
                    </Tabs>
                    <SitemapSettings/>
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