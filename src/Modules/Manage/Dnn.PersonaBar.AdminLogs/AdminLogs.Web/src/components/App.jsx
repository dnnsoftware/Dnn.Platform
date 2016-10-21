import React, { Component, PropTypes } from "react";
import PersonaBarPagesContainer from "../containers/personaBarPagesContainer";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import { visiblePanel as VisiblePanelActions } from "../actions";
require("es6-object-assign").polyfill();
require("array.prototype.find").shim();
require("array.prototype.findindex").shim();
import resx from "../resources";
import AdminLogs from "./AdminLog";
import LogSettings from "./LogSettings";
import {
    pagination as PaginationActions
} from "../actions";

class App extends Component {
    constructor() {
        super();
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    navigateMap(page, index, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
    }
    render() {
        const {props} = this;
        return (
            <PersonaBarPagesContainer pages={[
                <div>
                    <SocialPanelHeader title={("Admin Logs")}>
                    </SocialPanelHeader>
                    <SocialPanelBody>
                        <Tabs onSelect={this.handleSelect.bind(this)}
                            tabHeaders={[resx.get("AdminLogs.Header"), resx.get("LogSettings.Header")]}
                            type="primary">
                            <AdminLogs />
                            <LogSettings />
                        </Tabs>
                    </SocialPanelBody>
                </div>]}
                selectedPage={props.selectedPage}
                selectedPageVisibleIndex={props.selectedPageVisibleIndex}
                repaintChildren={true} />
        );
    }
}
App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);