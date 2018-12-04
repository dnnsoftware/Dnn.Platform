import React, { Component } from "react";
import PropTypes from "prop-types";
import { Scrollbars } from "react-custom-scrollbars";
import GridCell from "dnn-grid-cell";
import Dropdown from "dnn-dropdown";

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

    UNSAFE_componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    render() {
        const { props } = this;

        return <div className="dnn-servers-info-panel-big logsTab">
            <GridCell columnSize={60} className="log-file-cell">
                {props.logs.length > 0 &&
                    <Dropdown
                        withBorder={false}
                        label={Localization.get("Logs_LogFilesDefaultOption")}
                        options={props.logs}
                        value={props.selectedLog}
                        prependWith={Localization.get("Logs_LogFiles")}
                        onSelect={this.props.onSelectedLog}
                    />}
            </GridCell>
            <div className="clear" />
            <div>
                <Scrollbars
                    renderTrackHorizontal={props => <div {...props} className="track-horizontal"/>}
                    style={{ height: 500 }} >
                    <div className="log-file-display">{props.logData}</div>
                </Scrollbars>
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
        ...bindActionCreators({
            onRetrieveLogsServerInfo: LogsTabActions.loadLogsServerInfo,
            onSelectedLog: LogsTabActions.loadSelectedLog
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Logs);