import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    task as TaskActions
} from "../../actions";
import SchedulerRow from "./schedulerRow";
import { Dropdown, Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import SchedulerEditor from "./schedulerEditor";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class SchedulerPanel extends Component {
    constructor() {
        super();
        this.state = {
            currentServerId: "*",
            schedulerItemList: [],
            openId: "",
            serverList: [],
            historyPanelOpen: false
        };
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        props.dispatch(TaskActions.getSchedulerItemList());
        if (props.serverList === null || props.serverList === [] || props.serverList === undefined) {
            props.dispatch(TaskActions.getServerList());
        }

        tableFields = [];
        tableFields.push({ "name": resx.get("Name.Header"), "id": "FriendlyName" });
        tableFields.push({ "name": resx.get("Frequency.Header"), "id": "Frequency" });
        tableFields.push({ "name": resx.get("RetryTimeLapse.Header"), "id": "RetryTimeLapse" });
        tableFields.push({ "name": resx.get("NextStart.Header"), "id": "NextStart" });
        tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });
    }

    createServerOptions(serverList) {
        let serverOptions = [];
        if (serverList !== undefined) {
            serverOptions = serverList.map((item) => {
                return { label: item.ServerName, value: item.ServerID };
            });
        }
        return serverOptions;
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }
    uncollapse(id, index) {
        this.setState({
            openId: id,
            historyPanelOpen: index === 1
        });
    }
    collapse(index) {
        if (this.state.openId !== "") {
            this.setState({
                openId: "",
                historyPanelOpen: index !== 1
            });
        }
    }
    toggle(openId, index) {
        if (openId !== "") {
            this.uncollapse(openId, index);
        }
    }

    onSelectServer(event) {
        const { props } = this;
        let { state } = this;
        let index = event.target.selectedIndex;
        let server = event.target[index].text;
        let serverId = event.target.value;
        if (serverId !== state.currentServerId) {
            state.currentServerId = serverId;
            this.setState({
                currentServerId: state.currentServerId
            }, () => {
                props.dispatch(TaskActions.getSchedulerItemList({ serverName: server }));
            });
        }
    }

    onUpdateSchedulerItem(scheduleItemDetail) {
        const { props } = this;
        if (scheduleItemDetail.ScheduleID) {
            props.dispatch(TaskActions.updateScheduleItem(scheduleItemDetail, () => {
                util.utilities.notify(resx.get("ScheduleItemUpdateSuccess"));
                this.collapse();
                props.dispatch(TaskActions.getSchedulerItemList());
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(TaskActions.createScheduleItem(scheduleItemDetail, () => {
                util.utilities.notify(resx.get("ScheduleItemCreateSuccess"));
                this.collapse();
                props.dispatch(TaskActions.getSchedulerItemList());
            }, (error) => {
                util.utilities.notify(resx.get("ScheduleItemCreateError"));
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteSchedulerItem(scheduleId) {
        const { props } = this;
        util.utilities.confirm(resx.get("ScheduleItemDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const itemList = props.schedulerItemList.filter((item) => item.ScheduleID !== scheduleId);
            props.dispatch(TaskActions.deleteSchedule({ "ScheduleId": scheduleId }, itemList, () => {
                util.utilities.notify(resx.get("DeleteSuccess"));
                this.collapse();
            }, () => {
                util.utilities.notify(resx.get("DeleteError"));
            }));
        });
    }

    /* eslint-disable react/no-danger */
    renderedScedulerItemList() {
        let i = 0;
        if (this.props.schedulerItemList) {
            return this.props.schedulerItemList.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <SchedulerRow
                        scheduleId={item.ScheduleID}
                        name={item.FriendlyName}
                        frequency={item.Frequency}
                        retryTimeLapse={item.RetryTimeLapse}
                        nextStart={item.NextStart}
                        enabled={item.Enabled}
                        index={index}
                        key={"scheduleItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}
                        id={id}
                        panelIndex={this.state.historyPanelOpen ? 1 : 0}>
                        <SchedulerEditor
                            serverList={this.props.serverList}
                            enabled={item.Enabled}
                            scheduleId={item.ScheduleID}
                            Collapse={this.collapse.bind(this)}
                            onDelete={this.onDeleteSchedulerItem.bind(this)}
                            onUpdate={this.onUpdateSchedulerItem.bind(this)}
                            id={id}
                            openId={this.state.openId}
                            panelIndex={this.state.historyPanelOpen ? 1 : 0} />                        
                    </SchedulerRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        let serverOptions = this.createServerOptions(this.props.serverList);
        return (
            <div>
                <div className="schedule-items">
                    <div className="servergroup-filter-container">
                        {serverOptions.length > 2 &&
                            <Dropdown
                                options={serverOptions}
                                style={{ width: "100%" }}
                                withBorder={false}
                                value={this.state.currentServerId}
                                onSelect={this.onSelectServer.bind(this)}
                            />
                        }
                    </div>
                    <div className="AddItemRow">
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }}>
                            </div> {resx.get("cmdAddTask")}
                        </div>
                    </div>
                    <div className="schedule-items-grid">
                        {this.renderHeader()}
                        <Collapsible isOpened={opened} style={{ float: "left" }} fixedHeight={650}>
                            <SchedulerRow
                                name={"-"}
                                frequency={"-"}
                                retryTimeLapse={"-"}
                                nextStart={"-"}
                                index={"add"}
                                key={"scheduleItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"add"}>
                                <SchedulerEditor
                                    serverList={this.props.serverList}
                                    Collapse={this.collapse.bind(this)}
                                    onDelete={this.onDeleteSchedulerItem.bind(this)}
                                    onUpdate={this.onUpdateSchedulerItem.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </SchedulerRow>
                        </Collapsible>
                        {this.renderedScedulerItemList()}
                    </div>
                </div>

            </div>
        );
    }
}

SchedulerPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    schedulerItemList: PropTypes.array,
    serverList: PropTypes.array
};

function mapStateToProps(state) {
    return {
        schedulerItemList: state.task.schedulerItemList,
        serverList: state.task.serverList,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(SchedulerPanel);