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

class SmtpServer extends Component {

    componentDidMount() {
        this.props.onRetrieveSmtpServerInfo();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.infoMessage !== newProps.infoMessage && newProps.infoMessage) {
            utils.notify(newProps.infoMessage);
        }

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

    onSave() {
        const {props} = this;

        if (this.areThereValidationError()) {
            return;
        } 


        const smtpSettings = props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser() ? props.smtpServerInfo.host 
            : props.smtpServerInfo.site;

        const updateRequest = {
            smtpServerMode: props.smtpServerInfo.smtpServerMode,
            smtpServer: smtpSettings.smtpServer,
            smtpConnectionLimit: smtpSettings.smtpConnectionLimit,
            smtpMaxIdleTime: smtpSettings.smtpMaxIdleTime,
            smtpAuthentication: smtpSettings.smtpAuthentication,
            smtpUsername: smtpSettings.smtpUserName,
            smtpPassword: smtpSettings.smtpPassword,
            smtpHostEmail: smtpSettings.smtpHostEmail,
            enableSmtpSsl: smtpSettings.enableSmtpSsl,
            messageSchedulerBatchSize: props.smtpServerInfo.host.messageSchedulerBatchSize
        };
        props.onUpdateSmtpServerSettings(updateRequest);
    }

    areThereValidationError() {
        let areErrors = false;
        const errors = this.props.errors;
        for (let prop in errors) {
            if (errors[prop]) {
                return true;
            }
        }

        return areErrors;
    }

    onTestSmtpSettings() {
        const {props} = this;

        if (this.areThereValidationError()) {
            return;
        } 

        let smtpSettings = {};
        if (props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser()) {
            smtpSettings = props.smtpServerInfo.host;
        }
        if (props.smtpServerInfo.smtpServerMode === "p") {
            smtpSettings = props.smtpServerInfo.site;
        }
        
        const sendEmailRequest = {
            smtpServerMode: props.smtpServerInfo.smtpServerMode,
            smtpServer: smtpSettings.smtpServer,
            smtpAuthentication: smtpSettings.smtpAuthentication,
            smtpUsername: smtpSettings.smtpUserName,
            smtpPassword: smtpSettings.smtpPassword,
            enableSmtpSsl: smtpSettings.enableSmtpSsl
        };
        props.onSendTestEmail(sendEmailRequest);    
    }   

    getSmtpServerOptions() {
        return [{
            label: localization.get("GlobalSmtpHostSetting"),
            value: "h"
        },
        {
            label: localization.get("SiteSmtpHostSetting").replace("{0}", this.props.smtpServerInfo.portalName || ""),
            value: "p"
        }
        ];
    }

    getSmtpAuthenticationOptions() {
        return [{
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
    }


    render() {
        const {props} = this;
        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
        const selectedSmtpSettings = (areGlobalSettings ? props.smtpServerInfo.host : props.smtpServerInfo.site) || {};
        const credentialVisible = selectedSmtpSettings.smtpAuthentication === "1";
        const smtpSettingsVisible = utils.isHostUser() || !areGlobalSettings;

        return <div className="dnn-servers-info-panel-big smtpServerSettingsTab">
            <GridSystem>
                <div className="leftPane">
                    <div className="tooltipAdjustment border-bottom">
                        <RadioButtonBlock options={this.getSmtpServerOptions()}
                            label={localization.get("plSMTPMode")}
                            tooltip={localization.get("plSMTPMode.Help")}
                            onChange={this.onChangeSmtpServerMode.bind(this)}
                            value={props.smtpServerInfo.smtpServerMode} />
                    </div>
                    <div className="tooltipAdjustment">
                        {smtpSettingsVisible && 
                            <div>
                                <EditBlock label={localization.get("plSMTPServer")}
                                    tooltip={localization.get("plSMTPServer.Help")}
                                    value={selectedSmtpSettings.smtpServer}
                                    isGlobal={areGlobalSettings} 
                                    onChange={this.onChangeField.bind(this, "smtpServer")} 
                                    error={props.errors["smtpServer"]} />
                        
                                <EditBlock label={localization.get("plConnectionLimit")}
                                    tooltip={localization.get("plConnectionLimit.Help")}
                                    value={selectedSmtpSettings.smtpConnectionLimit} 
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpConnectionLimit")} 
                                    error={props.errors["smtpConnectionLimit"]} />
                        
                                <EditBlock label={localization.get("plMaxIdleTime")}
                                    tooltip={localization.get("plMaxIdleTime.Help")}
                                    value={selectedSmtpSettings.smtpMaxIdleTime} 
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpMaxIdleTime")}
                                    error={props.errors["smtpMaxIdleTime"]} />
                            </div>
                        }
                        {smtpSettingsVisible && areGlobalSettings &&
                            <EditBlock label={localization.get("plBatch")}
                                tooltip={localization.get("plBatch.Help")}
                                value={props.smtpServerInfo.host.messageSchedulerBatchSize} 
                                isGlobal={areGlobalSettings}
                                onChange={this.onChangeField.bind(this, "messageSchedulerBatchSize")}
                                error={props.errors["messageSchedulerBatchSize"]} />
                        }
                    </div>
                </div>
                <div className="rightPane">
                    {smtpSettingsVisible &&
                        <div className="tooltipAdjustment border-bottom">
                            <RadioButtonBlock options={this.getSmtpAuthenticationOptions()}
                                    label={localization.get("plSMTPAuthentication")}
                                    tooltip={localization.get("plSMTPAuthentication.Help")}
                                    onChange={this.onChangeAuthenticationMode.bind(this)}
                                    value={selectedSmtpSettings.smtpAuthentication || "0"} 
                                    isGlobal={areGlobalSettings} />
                        </div>
                    }
                    <div className="tooltipAdjustment border-bottom">
                        {smtpSettingsVisible && credentialVisible && 
                            <div>
                                <EditBlock label={localization.get("plSMTPUsername")}
                                    tooltip={localization.get("plSMTPUsername.Help")}
                                    value={selectedSmtpSettings.smtpUserName} 
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpUserName")} 
                                    error={props.errors["smtpUserName"]} />                   
                            
                                <EditBlock label={localization.get("plSMTPPassword")}
                                    tooltip={localization.get("plSMTPPassword.Help")}
                                    value={selectedSmtpSettings.smtpPassword} 
                                    isGlobal={areGlobalSettings} 
                                    type="password"
                                    onChange={this.onChangeField.bind(this, "smtpPassword")}
                                    error={props.errors["smtpPassword"]}  />
                            </div>     
                        }
                        {smtpSettingsVisible &&
                        <SwitchBlock label={localization.get("plSMTPEnableSSL")}
                            onText={localization.get("SwitchOn")}
                            offText={localization.get("SwitchOff")}
                            tooltip={localization.get("plSMTPEnableSSL.Help")}
                            value={selectedSmtpSettings.enableSmtpSsl}
                            onChange={this.onChangeSmtpEnableSsl.bind(this)}
                            isGlobal={areGlobalSettings} />
                        }              
                    </div>
                    {smtpSettingsVisible && areGlobalSettings &&
                        <EditBlock label={localization.get("plHostEmail")}
                            tooltip={localization.get("plHostEmail.Help")}
                            value={selectedSmtpSettings.smtpHostEmail}
                            isGlobal={true}
                            onChange={this.onChangeField.bind(this, "smtpHostEmail")}
                            error={props.errors["smtpHostEmail"]} />
                    }
                </div>
            </GridSystem>
            <div className="clear" />
            <div className="buttons-panel">
                 <Button type="secondary"
                    onClick={this.onTestSmtpSettings.bind(this)}>{localization.get("EmailTest")}</Button>
                 <Button type="primary" 
                    onClick={this.onSave.bind(this)}>{localization.get("SaveButtonText")}</Button>
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
    onChangeSmtpConfigurationValue: PropTypes.func.isRequired,
    onUpdateSmtpServerSettings: PropTypes.func.isRequired,
    infoMessage: PropTypes.string,
    onSendTestEmail: PropTypes.func.isRequired,
    errors: PropTypes.array
};

function mapStateToProps(state) {    
    return {
        smtpServerInfo: state.smtpServer.smtpServerInfo,
        errorMessage: state.smtpServer.errorMessage,
        infoMessage: state.smtpServer.infoMessage,
        errors: state.smtpServer.errors
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRetrieveSmtpServerInfo: SmtpServerTabActions.loadSmtpServerInfo,
            onChangeSmtpServerMode: SmtpServerTabActions.changeSmtpServerMode,
            onChangeSmtpAuthentication: SmtpServerTabActions.changeSmtpAuthentication,
            onChangeSmtpConfigurationValue: SmtpServerTabActions.changeSmtpConfigurationValue,
            onUpdateSmtpServerSettings: SmtpServerTabActions.updateSmtpServerSettings,
            onSendTestEmail: SmtpServerTabActions.sendTestEmail
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(SmtpServer);
