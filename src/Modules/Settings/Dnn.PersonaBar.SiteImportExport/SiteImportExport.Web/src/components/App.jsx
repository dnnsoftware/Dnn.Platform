import React, { Component, PropTypes } from "react";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import GridCell from "dnn-grid-cell";
import { connect } from "react-redux";
import {
    visiblePanel as VisiblePanelActions,
    pagination as PaginationActions,
    importExport as ImportExportActions
} from "../actions";
import PersonaBarPage from "dnn-persona-bar-page";
import Localization from "localization";
import Dashboard from "./Dashboard";
import ExportModal from "./ExportModal";
import ImportModal from "./ImportModal";
import util from "../utils";

let isHost = false;

class App extends Component {
    constructor() {
        super();
        isHost = util.settings.isHost;
        this.state = {
            portalId: -1
        };
    }

    componentWillMount() {
        const { props } = this;

        let pid = props.portalId && props.portalId > -1 ? props.portalId : util.settings.portalId;
        this.setState({
            portalId: pid
        }, () => {
            props.dispatch(ImportExportActions.siteSelected(pid));
        });
    }

    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
    }

    render() {
        const { state, props } = this;
        return (
            <div>
                <PersonaBarPage isOpen={props.selectedPage === 0} className={(props.selectedPage !== 0 ? "hidden" : "")}>
                    <PersonaBarPageHeader title={Localization.get("SiteImportExport.Header")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody>
                        <Dashboard selectPanel={this.selectPanel.bind(this)} />
                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <PersonaBarPageHeader title={Localization.get("Export")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: Localization.get("BackToImportExport"),
                        onClick: this.selectPanel.bind(this, 0)
                    }}>
                        <ExportModal onCancel={this.selectPanel.bind(this, 0)} />
                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 2}>
                    <PersonaBarPageHeader title={Localization.get("Import")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: Localization.get("BackToImportExport"),
                        onClick: this.selectPanel.bind(this, 0)
                    }}>
                        <ImportModal onCancel={this.selectPanel.bind(this, 0)} />
                    </PersonaBarPageBody>
                </PersonaBarPage>
            </div>
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