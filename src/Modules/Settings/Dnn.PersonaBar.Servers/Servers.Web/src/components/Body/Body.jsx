import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    pagination as PaginationActions
} from "../../actions";
import SocialPanelBody from "dnn-social-panel-body";
import "./style.less";
import Localization from "../../localization";
import ApplicationTab from "../Tabs/Application";
import WebTab from "../Tabs/Web";
import DatabaseTab from "../Tabs/Database/Database";
import SmtpServerTab from "../Tabs/SmtpServer";
import PerformanceTab from "../Tabs/Performance";
import LogsTab from "../Tabs/Logs";
import application from "../../globals/application";

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);        
    }   

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));
    }

    insertElementsInArray(array, elements, propertyNameHasIndex, propertyNameHasValue) {
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i][propertyNameHasIndex]);
            
            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, elements[i][propertyNameHasValue]);
            }            
        }
    }

    getInteger(value) {
        if (value === 0) {
            return 0;
        }
        if (value) {
            return parseInt(value.toString());
        }
        return value;
    }  
   
    render() {
        const {props} = this;       
        const registeredTabs = application.getRegisteredServerTabs();

        const systemInfoTabHeaders = [Localization.get("tabApplicationTitle"), Localization.get("tabWebTitle"), Localization.get("tabDatabaseTitle")];
        const systemInfoTabBody = [<ApplicationTab />, <WebTab />, <DatabaseTab />]; 
        const serverSettingsTabHeaders = [Localization.get("tabSmtpServerTitle"), Localization.get("tabPerformanceTitle"), Localization.get("tabLogsTitle")];
        const serverSettingsTabBody = [<SmtpServerTab smtpSettings={{}} />, <PerformanceTab />, <LogsTab />];
        const mainTabHeaders = [Localization.get("tabSystemInfoTitle"), Localization.get("tabServerSettingsTitle")];
        const mainTabBody = [
            <Tabs tabHeaders={systemInfoTabHeaders}
                type="secondary">
                {systemInfoTabBody}
            </Tabs>,
            <Tabs tabHeaders={serverSettingsTabHeaders}
                type="secondary">
                {serverSettingsTabBody}
            </Tabs>];
        
        this.insertElementsInArray(mainTabHeaders, registeredTabs.filter(
            function (tab) {
                return !tab.parentIndex && tab.parentIndex !== 0;
            }), "order", "title");
        this.insertElementsInArray(mainTabBody, registeredTabs.filter(
            function (tab) {
                return !tab.parentIndex && tab.parentIndex !== 0;
            }), "order", "component");

        this.insertElementsInArray(systemInfoTabHeaders, registeredTabs.filter(
            function (tab) {
                return tab.parentIndex === 0;
            }), "order", "title");
        this.insertElementsInArray(systemInfoTabBody, registeredTabs.filter(
            function (tab) {
                return tab.parentIndex === 0;
            }), "order", "component");

        this.insertElementsInArray(serverSettingsTabHeaders, registeredTabs.filter(
            function (tab) {
                return tab.parentIndex === 1;
            }), "order", "title");
        this.insertElementsInArray(serverSettingsTabBody, registeredTabs.filter(
            function (tab) {
                return tab.parentIndex === 1;
            }), "order", "component");

        return (
            <SocialPanelBody>
                <Tabs className="dnn-servers-tab-panel" 
                    onSelect={this.handleSelect}
                    selectedIndex={props.tabIndex}
                    tabHeaders={mainTabHeaders}>
                       {mainTabBody}                   
                </Tabs>
                
            </SocialPanelBody >
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number   
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(Body);