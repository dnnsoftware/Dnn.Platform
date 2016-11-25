import React, { Component, PropTypes } from "react";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import Tabs from "dnn-tabs";
import GridCell from "dnn-grid-cell";
import { connect } from "react-redux";
import { visiblePanel as VisiblePanelActions } from "../actions";
import Localization from "localization";
import AdminLogs from "./AdminLog";
import LogSettings from "./LogSettings";
import {
    pagination as PaginationActions
} from "../actions";
import util from "../utils";

let isHost = false;

class App extends Component {
    constructor() {
        super();
        isHost = util.settings.isHost;
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
        return (
            <GridCell>
                <PersonaBarPageHeader title={("Admin Logs")}>
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    {isHost &&
                        <Tabs onSelect={this.handleSelect.bind(this)}
                            tabHeaders={[Localization.get("AdminLogs.Header"), Localization.get("LogSettings.Header")]}
                            type="primary">
                            <AdminLogs />
                            <LogSettings />
                        </Tabs>
                    }
                    {!isHost &&
                        <Tabs onSelect={this.handleSelect.bind(this)}
                            tabHeaders={[Localization.get("AdminLogs.Header")]}
                            type="primary">
                            <AdminLogs />
                        </Tabs>
                    }
                </PersonaBarPageBody>
            </GridCell>
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