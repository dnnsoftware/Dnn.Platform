import React, {Component, PropTypes } from "react";
import Label from "dnn-label";
import GridSystem from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import InputGroup from "dnn-input-group";
import MultiLineInput from "dnn-multi-line-input";
import Localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import LogsTabActions from "../../actions/logsTab";
import utils from "../../utils";
import "./tabs.less";

class Logs extends Component {   
    componentDidMount() {
        if (this.props.logs.length > 0) {
            return;
        }
        
        this.props.onRetrieveLogsServerInfo();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }
        
    render() {
        const {props} = this;

        return <div className="dnn-servers-info-panel-big logsTab">
            <GridSystem className="border-bottom">
                <div className="leftPane">
                    <InputGroup>
                        <Label className="title" 
                            label={Localization.get("Logs_LogFiles")} 
                            tooltipMessage={Localization.get("Logs_LogFilesTooltip")}/>
                        {props.logs.length > 0 && 
                        <Dropdown withBorder={false}
                            label={Localization.get("Logs_LogFilesDefaultOption")}
                            options={props.logs}
                            value={props.selectedLog}
                            onSelect={this.props.onSelectedLog}
                            />}
                    </InputGroup>
                </div>
                <div className="rightPane">
                </div>
            </GridSystem>
            <div className="clear" />
            <div>
                <MultiLineInput
                    value={props.logData}
                    />
            </div>
        </div>;
    }    
}

Logs.propTypes = {   
    logs: PropTypes.arrayOf(PropTypes.object),
    errorMessage: PropTypes.string,
    selectedLog: PropTypes.string,
    onRetrieveLogsServerInfo: PropTypes.func.isRequired,
    onSelectedLog: PropTypes.func,
    logData: PropTypes.func
};

function mapStateToProps(state) {    
    return {
        logs: state.logsTab.logs,
        errorMessage: state.logsTab.errorMessage,
        selectedLog: state.logsTab.selectedLog,
        logData: state.logsTab.logData
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRetrieveLogsServerInfo: LogsTabActions.loadLogsServerInfo,
            onSelectedLog: LogsTabActions.loadSelectedLog
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Logs);