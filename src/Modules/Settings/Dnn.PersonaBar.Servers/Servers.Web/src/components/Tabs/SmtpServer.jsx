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

class SmtpServer extends Component {

    constructor() {
        super();

        this.state = {
            smtpServerMode: "site",
            smtpAuthentication: "anonymous"
        };
    }

    componentDidMount() {
        this.props.onRetrieveSmtpServerInfo();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    onChangeSmtpServerMode(mode) {
        this.setState({ smtpServerMode: mode });
    }

    onChangeAuthenticationMode(authentication) {
        this.setState({ smtpAuthentication: authentication });
    }

    onChangeSmtpEnableSsl() {

    }

    onTestSmtpSettings() {

    }

    onSave() {

    }

    render() {
        const {props} = this;
        const areGlobalSettings = this.state.smtpServerMode === "global";
        const credentialVisible = this.state.smtpAuthentication === "basic";
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
                            value={props.smtpServerInfo.smtpServer}
                            isGlobal={areGlobalSettings} />
                   
                        <EditBlock label={localization.get("plConnectionLimit")}
                            tooltip={localization.get("plConnectionLimit.Help")}
                            value={props.smtpServerInfo.smtpConnectionLimit} 
                            isGlobal={areGlobalSettings} />
                   
                        <EditBlock label={localization.get("plMaxIdleTime")}
                            tooltip={localization.get("plMaxIdleTime.Help")}
                            value={props.smtpServerInfo.smtpMaxIdleTime} 
                            isGlobal={areGlobalSettings} />
                 
                        <EditBlock label={localization.get("plBatch")}
                            tooltip={localization.get("plBatch.Help")}
                            value={props.smtpServerInfo.messageSchedulerBatchSize} 
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
                        {credentialVisible && 
                            <div>
                                <EditBlock label={localization.get("plSMTPUsername")}
                                    tooltip={localization.get("plSMTPUsername.Help")}
                                    value={props.smtpServerInfo.smtpUserName} 
                                    isGlobal={areGlobalSettings} />                   
                            
                                <EditBlock label={localization.get("plSMTPPassword")}
                                    tooltip={localization.get("plSMTPPassword.Help")}
                                    value={props.smtpServerInfo.smtpPassword} 
                                    isGlobal={areGlobalSettings} />
                            </div>     
                        }
                        <SwitchBlock label={localization.get("plSMTPEnableSSL")}
                            tooltip={localization.get("plSMTPEnableSSL.Help")}
                            value={props.smtpServerInfo.enableSmtpSsl}
                            onChange={this.onChangeSmtpEnableSsl.bind(this)}
                            isGlobal={areGlobalSettings}  />              
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
    onRetrieveSmtpServerInfo: PropTypes.func.isRequired
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
            onRetrieveSmtpServerInfo: SmtpServerTabActions.loadSmtpServerInfo     
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(SmtpServer);
