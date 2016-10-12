import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    task as TaskActions
} from "../../actions";
import TaskQueue from "../taskQueue";
import History from "../history";
import Scheduler from "../scheduler";
import TopPane from "../topPane";
import SocialPanelBody from "dnn-social-panel-body";
import "./style.less";
import resx from "../../resources";

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <SocialPanelBody>
                <TopPane/>
                <Tabs onSelect={this.handleSelect.bind(this) }
                    tabHeaders={[resx.get("TabTaskQueue"), resx.get("TabScheduler"), resx.get("TabHistory")]}
                    type="primary">
                    <TaskQueue/>
                    <Scheduler/>
                    <History title={resx.get("TabHistoryTitle") } />
                </Tabs>
            </SocialPanelBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    status: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index,
        status: state.task.status
    };
}

export default connect(mapStateToProps)(Body);