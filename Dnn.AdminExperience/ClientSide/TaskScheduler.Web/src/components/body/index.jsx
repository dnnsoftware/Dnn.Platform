import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    task as TaskActions
} from "../../actions";
import TaskQueue from "../taskQueue";
import History from "../history";
import Scheduler from "../scheduler";
import TopPane from "../topPane";
import { DnnTabs as Tabs, PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../resources";
import util from "../../utils";

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
    }

    handleSelect(index) {
        const {props} = this;
        if (props.settingsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(TaskActions.cancelSettingsClientModified());      
                props.dispatch(PaginationActions.loadTab(index));
            });
        }
        else {
            props.dispatch(PaginationActions.loadTab(index));
        }
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <PersonaBarPageBody>
                <TopPane />
                <Tabs
                    onSelect={this.handleSelect.bind(this)}
                    selectedIndex={this.props.tabIndex}
                    tabHeaders={[resx.get("TabTaskQueue"), resx.get("TabScheduler"), resx.get("TabHistory")]}
                    type="primary">
                    <TaskQueue />
                    <Scheduler />
                    <History title={resx.get("TabHistoryTitle")} />
                </Tabs>
            </PersonaBarPageBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    status: PropTypes.string,
    settingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        status: state.task.status,
        settingsClientModified: state.task.settingsClientModified
    };
}

export default connect(mapStateToProps)(Body);