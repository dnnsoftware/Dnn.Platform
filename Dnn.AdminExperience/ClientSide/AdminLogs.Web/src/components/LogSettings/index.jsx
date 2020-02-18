import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import {
    logSettings as LogSettingActions,
    log as LogActions
} from "../../actions";
import SettingRow from "./LogSettingRow";
import LogSettingEditor from "./LogSettingEditor";
import "./style.less";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import Localization from "localization";
import {
    createPortalOptions,
    createLogTypeOptions
} from "../../reducerHelpers";


let canEdit = false;
class LogSettingsPanel extends Component {
    constructor() {
        super();
        this.state = {
            openId: ""
        };
        canEdit = util.settings.isHost || util.settings.permissions.LOG_SETTINGS_EDIT;
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(LogSettingActions.getLogSettings());
        if (canEdit) {
            if (this.props.logTypeList === null || this.props.logTypeList === undefined || this.props.logTypeList.length === 0) {
                props.dispatch(LogActions.getLogTypeList());
            }
            if (this.props.portalList === null || this.props.portalList === undefined || this.props.portalList.length === 0)
                props.dispatch(LogActions.getPortalList(util.settings.isHost));
            props.dispatch(LogSettingActions.getKeepMostRecentOptions());
            props.dispatch(LogSettingActions.getOccurrenceOptions());
        }
    }

    renderHeader() {
        const tableFields = [
            { "name": Localization.get("LogType.Header"), "width": 40 },
            { "name": Localization.get("Portal.Header"), "width": 20 },
            { "name": Localization.get("Active.Header"), "width": 15 },
            { "name": Localization.get("FileName.Header"), "width": 20 },
            { "name": "", "width": 5 }
        ];
        let tableHeaders = tableFields.map((field) => {
            return <GridCell key={field.name} columnSize={field.width} style={{ fontWeight: "bolder" }}>
                <span>{field.name}&nbsp; </span>
            </GridCell>;
        });
        return <div id="header-row" className="header-row">{tableHeaders}</div>;
    }
    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, 0);
    }
    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }
    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        } else {
            this.collapse();
        }
    }

    /* eslint-disable react/no-danger */
    renderedLogSettingList(logTypeOptions, portalOptions) {
        let validLogSettingList = this.props.logSettingList.filter(logSetting => !!logSetting);
        let i = 0;
        return validLogSettingList.map((logSetting, index) => {
            let id = "row-" + i++;
            return (
                <SettingRow
                    typeName={logSetting.LogTypeFriendlyName}
                    website={logSetting.LogTypePortalName}
                    activeStatus={logSetting.LoggingIsActive ? Localization.get("True") : Localization.get("False") }
                    fileName={logSetting.LogFileName}
                    logTypeKey={logSetting.LogTypeKey}
                    index={index}
                    key={"logSetting-" + index}
                    closeOnClick={true}
                    openId={this.state.openId }
                    OpenCollapse={this.toggle.bind(this) }
                    Collapse={this.collapse.bind(this) }
                    id={id}
                    readOnly={!canEdit}>
                    <LogSettingEditor
                        logTypeList={logTypeOptions }
                        portalList={portalOptions }
                        keepMostRecentOptions={this.props.keepMostRecentOptions}
                        thresholdsOptions={this.props.thresholdsOptions}
                        notificationTimesOptions={this.props.notificationTimesOptions}
                        notificationTimeTypesOptions={this.props.notificationTimeTypesOptions}
                        logTypeSettingId={logSetting.ID}  Collapse={this.collapse.bind(this) } id={id} openId={this.state.openId} />
                </SettingRow>
            );
        });
    }

    render() {
        let opened = (this.state.openId === "add");
        let logTypeOptions = createLogTypeOptions(this.props.logTypeList);
        let portalOptions = createPortalOptions(this.props.portalList);
        return (
            <div>
                <div className="log-settings">
                    {canEdit &&
                        <div className="add-setting-row" onClick={this.toggle.bind(this, opened ? "" : "add") }>
                            <div className={"add-setting-box " + !opened}>
                                <div className={"add-icon"} dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }}>
                                </div> {Localization.get("AddContent.Action") }
                            </div>
                        </div>
                    }
                    <div className="container">
                        {this.renderHeader() }
                        {canEdit && <div className="add-setting-editor">
                            <SettingRow
                                typeName={"-"}
                                website={"-"}
                                activeStatus={"-"}
                                fileName={""}
                                logTypeKey={"-"}
                                index={"add"}
                                key={"logSetting-add"}
                                closeOnClick={true}
                                openId={this.state.openId }
                                OpenCollapse={this.toggle.bind(this) }
                                Collapse={this.collapse.bind(this) }
                                id={"add"}
                                visible={opened}>
                                <LogSettingEditor
                                    logTypeList={logTypeOptions}
                                    portalList={portalOptions }
                                    keepMostRecentOptions={this.props.keepMostRecentOptions}
                                    thresholdsOptions={this.props.thresholdsOptions}
                                    notificationTimesOptions={this.props.notificationTimesOptions}
                                    notificationTimeTypesOptions={this.props.notificationTimeTypesOptions}
                                    logTypeSettingId=""  Collapse={this.collapse.bind(this) }  id={"add"} openId={this.state.openId}/>
                            </SettingRow>
                        </div>
                        }
                        {this.renderedLogSettingList(logTypeOptions, portalOptions) }
                    </div>
                </div>
            </div >
        );
    }
}

LogSettingsPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    logTypeList: PropTypes.array,
    logSettingList: PropTypes.array,
    portalList: PropTypes.array,
    logTypeSetting: PropTypes.object,
    keepMostRecentOptions: PropTypes.array.isRequired,
    thresholdsOptions: PropTypes.array.isRequired,
    notificationTimesOptions: PropTypes.array.isRequired,
    notificationTimeTypesOptions: PropTypes.array.isRequired
};

function mapStateToProps(state) {
    return {
        logSettingList: state.logSettings.logSettingList,
        logTypeList: state.log.logTypeList,
        portalList: state.log.portalList,
        keepMostRecentOptions: state.logSettings.keepMostRecentOptions,
        thresholdsOptions: state.logSettings.thresholdsOptions,
        notificationTimesOptions: state.logSettings.notificationTimesOptions,
        notificationTimeTypesOptions: state.logSettings.notificationTimeTypesOptions,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(LogSettingsPanel);