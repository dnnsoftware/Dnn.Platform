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

class App extends Component {
    constructor() {
        super();
        this.state = {
            referrer: util.settings.referrer,
            referrerText: util.settings.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, util.settings.backToReferrerFunc)
        };
    }

    backToReferrer(callback) {
        if (typeof callback === "function") {
            callback();
        }
        setTimeout(() => {
            this.setState({
                referrer: "",
                referrerText: "",
                backToReferrerFunc: null
            });
        }, 750);
    }

    updateReferrerInfo(event) {
        this.setState({
            referrer: event.referrer,
            referrerText: event.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, event.backToReferrerFunc)
        });
    }

    componentWillMount() {
        const { props, state } = this;

        document.addEventListener("siteImportExport", (e) => {
            props.dispatch(ImportExportActions.siteSelected(e.portalIdFromSites, () => {
                this.selectPanel(e.jobType === "Export" ? 1 : 2);
            }));
            this.updateReferrerInfo(e);
        }, false);

        if (util.settings.portalIdFromSites > -1) {
            props.dispatch(ImportExportActions.siteSelected(util.settings.portalIdFromSites, () => {
                this.selectPanel(util.settings.jobType === "Export" ? 1 : 2);
            }));
            this.updateReferrerInfo(util.settings);
        }
        if (props.portalId > -1 || state.portalId === -1) {
            this.setState({
                portalId: props.portalId
            }, () => {
                props.dispatch(ImportExportActions.siteSelected(props.portalId));
            });
        }
    }

    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
    }

    render() {
        const { props, state } = this;
        return (
            <div>
                <PersonaBarPage isOpen={props.selectedPage === 0} className={(props.selectedPage !== 0 ? "hidden" : "")}>
                    <PersonaBarPageHeader title={Localization.get("SiteImportExport.Header")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText,
                        onClick: state.backToReferrerFunc
                    }}>
                        {props.selectedPage === 0 && <Dashboard selectPanel={this.selectPanel.bind(this)} />}
                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <PersonaBarPageHeader title={Localization.get("Export")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText || Localization.get("BackToImportExport"),
                        onClick: state.backToReferrerFunc || this.selectPanel.bind(this, 0)
                    }}>
                        {props.selectedPage === 1 && <ExportModal onCancel={this.selectPanel.bind(this, 0)} />}
                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 2}>
                    <PersonaBarPageHeader title={Localization.get("Import")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText || Localization.get("BackToImportExport"),
                        onClick: state.backToReferrerFunc || this.selectPanel.bind(this, 0)
                    }}>
                        {props.selectedPage === 2 && <ImportModal onCancel={this.selectPanel.bind(this, 0)} />}
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
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        portalId: state.importExport.portalId
    };
}

export default connect(mapStateToProps)(App);