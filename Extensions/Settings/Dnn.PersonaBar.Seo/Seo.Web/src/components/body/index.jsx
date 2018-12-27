import React, { Component } from "react";
import PropTypes from "prop-types";
import { DnnTabs as Tabs, Tooltip, PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import { connect } from "react-redux";
import { pagination as PaginationActions } from "../../actions";
import GeneralSettings from "../generalSettings";
import RegexSettings from "../regexSettings";
import SitemapSettings from "../sitemapSettings";
import ExtensionUrlProviders from "../extensionUrlProviders";
import TestUrl from "../testUrl";
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
        if (isHost) {
            return <Tabs onSelect={this.handleSelect.bind(this)}
                tabHeaders={[resx.get("URLManagementTab"),
                    resx.get("SitemapSettingsTab")]}
                type="primary">
                <Tabs onSelect={this.handleSelect.bind(this)}
                    tabHeaders={[
                        resx.get("GeneralSettingsTab"), 
                        resx.get("ExtensionUrlProvidersTab"), 
                        <div style={{ fontSize: "9pt", paddingRight: 25 }} key="first">
                            {resx.get("ExpressionsTab")} 
                            <Tooltip
                                messages={[resx.get("GlobalSetting")]}
                                type="global"
                                style={{ position: "absolute", right: 0, top: 15, float: "right", textTransform: "none" }} />
                        </div>, 
                        resx.get("TestURLTab")
                    ]}
                    type="secondary">
                    <GeneralSettings />
                    <ExtensionUrlProviders />
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
                    tabHeaders={[resx.get("GeneralSettingsTab"), resx.get("ExtensionUrlProvidersTab"), resx.get("TestURLTab")]}
                    type="secondary">
                    <GeneralSettings />
                    <ExtensionUrlProviders />
                    <TestUrl />
                </Tabs>
                <SitemapSettings />
            </Tabs>;
        }
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
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index
    };
}

export default connect(mapStateToProps)(Body);