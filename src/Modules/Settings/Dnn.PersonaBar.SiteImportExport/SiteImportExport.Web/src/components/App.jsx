import React, { Component, PropTypes } from "react";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import GridCell from "dnn-grid-cell";
import { connect } from "react-redux";
import {
    visiblePanel as VisiblePanelActions,
    pagination as PaginationActions
} from "../actions";
import Localization from "localization";
import Dashboard from "./Dashboard";
import util from "../utils";

let isHost = false;

class App extends Component {
    constructor() {
        super();
        isHost = util.settings.isHost;
        this.state = {
            portalId: util.settings.portalId
        };
    }

    render() {
        const {state} = this;
        return (
            <GridCell>
                <PersonaBarPageHeader title={Localization.get("SiteImportExport.Header")}>
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <Dashboard portalId={state.portalId} />
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}
App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);