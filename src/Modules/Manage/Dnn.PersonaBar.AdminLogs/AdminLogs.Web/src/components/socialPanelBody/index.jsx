import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import {
    pagination as PaginationActions
} from "../../actions";
import AdminLogs from "../AdminLog";
import LogSettings from "../LogSettings";
import "./style.less";
import resx from "../../resources";

export class SocialPanelBody extends Component {
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
            <div className="socialpanelbody">
                <div>
                    <div className="normalPanel">
                        <div className="searchpanel">
                            <Tabs onSelect={this.handleSelect.bind(this) }
                                tabHeaders={[resx.get("AdminLogs.Header"), resx.get("LogSettings.Header")]}
                                type="primary">
                                <AdminLogs />
                                <LogSettings />
                            </Tabs>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

SocialPanelBody.propTypes = {
    subList: PropTypes.node,
    tabIndex: PropTypes.tabIndex
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(SocialPanelBody);