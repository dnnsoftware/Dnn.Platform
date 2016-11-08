import React, { Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import RadioButtonBlock from "../common/RadioButtonBlock";
import EditBlock from "../common/EditBlock";
import SwitchBlock from "../common/SwitchBlock";
import localization from "../../localization";
import Button from "dnn-button";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import SmtpServerTabActions from "../../actions/smtpServerTab";
import utils from "../../utils";

const smtpServerOptions = [
    {
        label: localization.get("GlobalSmtpHostSetting"),
        value: "h"
    },
    {
        label: localization.get("SiteSmtpHostSetting").replace("{0}", "Test site"),
        value: "p"
    }
];

const smtpAuthenticationOptions = [
    {
        label: localization.get("SMTPAnonymous"),
        value: "0"
    },
    {
        label: localization.get("SMTPBasic"),
        value: "1"
    },
    {
        label: localization.get("SMTPNTLM"),
        value: "2"
    }
];

class SmtpServer extends Component {

    componentDidMount() {
        this.props.onRetrieveSmtpServerInfo();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    onChangeSmtpServerMode(mode) {
        this.props.onChangeSmtpServerMode(mode);
    }

    onChangeAuthenticationMode(authentication) {
        this.props.onChangeSmtpAuthentication(authentication);
    }

    onChangeSmtpEnableSsl(enabled) {
        this.props.onChangeSmtpConfigurationValue("enableSmtpSsl", enabled);
    }

    onChangeField(key, event) {
        this.props.onChangeSmtpConfigurationValue(key, event.target.value);
    }

    onTestSmtpSettings() {
       
    }

    onSave() {

    }

    render() {
        const {props} = this;
        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
        const selectedSmtpSettings = (areGlobalSettings ? props.smtpServerInfo.host : props.smtpServerInfo.site) || {};
        const credentialVisible = selectedSmtpSettings.smtpAuthentication === "1";

        return <div className="dnn-servers-info-panel-big">
            <GridSystem>
                <div className="leftPane">
                    <div className="tooltipAdjustment border-bottom">
                        <RadioButtonBlock options={smtpServerOptions}
                            label={localization.get("plSMTPMode")}
                            tooltip={localization.get("plSMTPMode.Help")}
                            onChange={this.onChangeSmtpServerMode.bind(this)}
                            value={props.smtpServerInfo.smtpServerMode} />
                    </div>
                    <div className="tooltipAdjustment">
                        <EditBlock label={localization.get("plSMTPServer")}
                            tooltip={localization.get("plSMTPServer.Help")}
                            value={selectedSmtpSettings.smtpServer}
                            isGlobal={areGlobalSettings} 
                            onChange={this.onChangeField.bind(this, "smtpServer")} />
                   
                        <EditBlock label={localization.get("plConnectionLimit")}
                            tooltip={localization.get("plConnectionLimit.Help")}
                            value={selectedSmtpSettings.smtpConnectionLimit} 
                            isGlobal={areGlobalSettings}
                            onChange={this.onChangeField.bind(this, "smtpConnectionLimit")} />
                   
                        <EditBlock label={localization.get("plMaxIdleTime")}
                            tooltip={localization.get("plMaxIdleTime.Help")}
                            value={selectedSmtpSettings.smtpMaxIdleTime} 
                            isGlobal={areGlobalSettings}
                            onChange={this.onChangeField.bind(this, "smtpMaxIdleTime")} />
                 
                        {areGlobalSettings &&
                            <EditBlock label={localization.get("plBatch")}
                                tooltip={localization.get("plBatch.Help")}
                                value={props.smtpServerInfo.host.messageSchedulerBatchSize} 
                                isGlobal={areGlobalSettings}
                                onChange={this.onChangeField.bind(this, "messageSchedulerBatchSize")} />
                        }
                    </div>
                </div>
                <div className="rightPane">
                    <div className="tooltipAdjustment border-bottom">
                        <RadioButtonBlock options={smtpAuthenticationOptions}
                                label={localization.get("plSMTPAuthentication")}
                                tooltip={localization.get("plSMTPAuthentication.Help")}
                                onChange={this.onChangeAuthenticationMode.bind(this)}
                                value={selectedSmtpSettings.smtpAuthentication} 
                                isGlobal={areGlobalSettings} />
                    </div>
                    <div className="tooltipAdjustment">
                        {credentialVisible && 
                            <div>
                                <EditBlock label={localization.get("plSMTPUsername")}
                                    tooltip={localization.get("plSMTPUsername.Help")}
                                    value={selectedSmtpSettings.smtpUserName} 
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpUserName")} />                   
                            
                                <EditBlock label={localization.get("plSMTPPassword")}
                                    tooltip={localization.get("plSMTPPassword.Help")}
                                    value={selectedSmtpSettings.smtpPassword} 
                                    isGlobal={areGlobalSettings} 
                                    type="password"
                                    onChange={this.onChangeField.bind(this, "smtpPassword")} />
                            </div>     
                        }
                        <SwitchBlock label={localization.get("plSMTPEnableSSL")}
                            tooltip={localization.get("plSMTPEnableSSL.Help")}
                            value={selectedSmtpSettings.enableSmtpSsl}
                            onChange={this.onChangeSmtpEnableSsl.bind(this)}
                            isGlobal={areGlobalSettings} />              
                    </div>
                </div>
            </GridSystem>
            <div className="clear" />
            <div className="buttons-panel">
                 <Button type="secondary"
                    onClick={props.onTestSmtpSettings}>{localization.get("EmailTest")}</Button>
                 <Button type="primary" 
                    onClick={props.onSave}>{localization.get("SaveButtonText")}</Button>
            </div>
        </div>;
    }
}


SmtpServer.propTypes = {   
    smtpServerInfo: PropTypes.object.isRequired,
    errorMessage: PropTypes.string,
    onRetrieveSmtpServerInfo: PropTypes.func.isRequired,
    onChangeSmtpServerMode: PropTypes.func.isRequired,
    onChangeSmtpAuthentication: PropTypes.func.isRequired,
    onChangeSmtpConfigurationValue: PropTypes.func.isRequired
};

function mapStateToProps(state) {    
    return {
        smtpServerInfo: state.smtpServer.smtpServerInfo,
        errorMessage: state.webTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRetrieveSmtpServerInfo: SmtpServerTabActions.loadSmtpServerInfo,
            onChangeSmtpServerMode: SmtpServerTabActions.changeSmtpServerMode,
            onChangeSmtpAuthentication: SmtpServerTabActions.changeSmtpAuthentication,
            onChangeSmtpConfigurationValue: SmtpServerTabActions.changeSmtpConfigurationValue
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(SmtpServer);
