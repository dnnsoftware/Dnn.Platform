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
import DatabaseTab from "../Tabs/Database";
import SmtpServerTab from "../Tabs/SmtpServer";
import PerformanceTab from "../Tabs/Performance";
import LogsTab from "../Tabs/Logs";

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);        
    }   

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    render() {
        const {props} = this;

        return (
            <SocialPanelBody>
                <Tabs onSelect={this.handleSelect}
                    selectedIndex={props.tabIndex}
                    tabHeaders={[
                        Localization.get("tabSystemInfoTitle"), 
                        Localization.get("tabServerSettingsTitle")]}>
                        <Tabs tabHeaders={[
                            Localization.get("tabApplicationTitle"), 
                            Localization.get("tabWebTitle"), 
                            Localization.get("tabDatabaseTitle")]}
                            type="secondary">
                            <ApplicationTab />
                            <WebTab />
                            <DatabaseTab />
                        </Tabs>
                        <Tabs tabHeaders={[
                            Localization.get("tabSmtpServerTitle"), 
                            Localization.get("tabPerformanceTitle"), 
                            Localization.get("tabLogsTitle")]}
                            type="secondary">          
                            <SmtpServerTab />
                            <PerformanceTab />
                            <LogsTab />              
                        </Tabs>                    
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