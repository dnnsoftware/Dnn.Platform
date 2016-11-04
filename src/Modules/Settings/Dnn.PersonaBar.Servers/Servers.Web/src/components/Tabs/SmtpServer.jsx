import React, { Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import RadioButtonBlock from "../common/RadioButtonBlock";
import EditBlock from "../common/EditBlock";
import SwitchBlock from "../common/SwitchBlock";
import localization from "../../localization";

const smtpServerOptions = [
    {
        label: localization.get("GlobalSmtpHostSetting"),
        value: "global"
    },
    {
        label: localization.get("SiteSmtpHostSetting").replace("{0}", "Test site"),
        value: "site"
    }
];

const smtpAuthenticationOptions = [
    {
        label: localization.get("SMTPAnonymous"),
        value: "anonymous"
    },
    {
        label: localization.get("SMTPBasic"),
        value: "basic"
    },
    {
        label: localization.get("SMTPNTLM"),
        value: "ntlm"
    }
];

export default class SmtpServer extends Component {

    constructor() {
        super();

        this.state = {
            smtpServerMode: "site",
            smtpAuthentication: "anonymous"
        };
    }

    onChangeSmtpServerMode(mode) {
        this.setState({ smtpServerMode: mode });
    }

    onChangeAuthenticationMode(authentication) {
        this.setState({ smtpAuthentication: authentication });
    }

    onChangeSmtpEnableSsl() {

    }

    render() {
        const {props} = this;
        const areGlobalSettings = this.state.smtpServerMode === "global";
        return <div className="dnn-servers-info-panel-big">
            <GridSystem>
                <div className="leftPane">
                    <div className="tooltipAdjustment border-bottom">
                        <RadioButtonBlock options={smtpServerOptions}
                            label={localization.get("plSMTPMode")}
                            tooltip={localization.get("plSMTPMode.Help")}
                            onChange={this.onChangeSmtpServerMode.bind(this)}
                            value={this.state.smtpServerMode} />
                    </div>
                    <div className="tooltipAdjustment">
                        <EditBlock label={localization.get("plSMTPServer")}
                            tooltip={localization.get("plSMTPServer.Help")}
                            value={props.smtpSettings.smtpServer}
                            isGlobal={areGlobalSettings} />
                   
                        <EditBlock label={localization.get("plConnectionLimit")}
                            tooltip={localization.get("plConnectionLimit.Help")}
                            value={props.smtpSettings.smtpConnectionLimit} 
                            isGlobal={areGlobalSettings} />
                   
                        <EditBlock label={localization.get("plMaxIdleTime")}
                            tooltip={localization.get("plMaxIdleTime.Help")}
                            value={props.smtpSettings.smtpMaxIdleTime} 
                            isGlobal={areGlobalSettings} />
                 
                        <EditBlock label={localization.get("plBatch")}
                            tooltip={localization.get("plBatch.Help")}
                            value={props.smtpSettings.messageSchedulerBatchSize} 
                            isGlobal={areGlobalSettings} />
                    </div>
                </div>
                <div className="rightPane">
                    <div className="tooltipAdjustment border-bottom">
                        <RadioButtonBlock options={smtpAuthenticationOptions}
                                label={localization.get("plSMTPAuthentication")}
                                tooltip={localization.get("plSMTPAuthentication.Help")}
                                onChange={this.onChangeAuthenticationMode.bind(this)}
                                value={this.state.smtpAuthentication} 
                                isGlobal={areGlobalSettings} />
                    </div>
                    <div className="tooltipAdjustment">
                        <EditBlock label={localization.get("plSMTPUsername")}
                            tooltip={localization.get("plSMTPUsername.Help")}
                            value={props.smtpSettings.smtpUserName} 
                            isGlobal={areGlobalSettings} />                   
                    
                        <EditBlock label={localization.get("plSMTPPassword")}
                            tooltip={localization.get("plSMTPPassword.Help")}
                            value={props.smtpSettings.smtpPassword} 
                            isGlobal={areGlobalSettings} />     

                        <SwitchBlock label={localization.get("plSMTPEnableSSL")}
                            tooltip={localization.get("plSMTPEnableSSL.Help")}
                            value={props.smtpSettings.enableSmtpSsl}
                            onChange={this.onChangeSmtpEnableSsl.bind(this)}
                            isGlobal={areGlobalSettings}  />              
                    </div>
                </div>
            </GridSystem>
        </div>;
    }
}

SmtpServer.propTypes = {
    smtpSettings: PropTypes.object
};