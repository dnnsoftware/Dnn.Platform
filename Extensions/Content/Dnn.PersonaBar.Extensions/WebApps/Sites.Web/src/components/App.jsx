import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import PortalList from "./PortalList";
import { CommonVisiblePanelActions, CommonExportPortalActions } from "dnn-sites-common-actions";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import {ExportPortal} from "dnn-sites-common-components";
import CreatePortal from "./CreatePortal";

class App extends Component {
    constructor() {
        super();
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
    }

    onAddNewSite(event) {
        event.preventDefault();
        this.navigateMap(1);
    }

    onEditSite() { }

    onExportPortal(portalBeingExported) {
        const { props } = this;
        props.dispatch(CommonExportPortalActions.setPortalBeingExported(portalBeingExported, this.navigateMap.bind(this, 2)));
    }

    navigateMap(page) {
        const {props} = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
    }

    cancelExport(event) {
        if (event !== undefined)
            event.preventDefault();
        this.setState({
            portalBeingExported: {}
        });
        this.navigateMap(0);
    }

    render() {
        const {props} = this;
        return (
            <div className="sites-Root">
                <PersonaBarPage isOpen={props.selectedPage === 0 || props.selectedPage === 2}>
                    <PortalList
                        onAddNewSite={this.onAddNewSite.bind(this)}
                        onExportPortal={this.onExportPortal.bind(this)} />
                </PersonaBarPage>
                {props.selectedPage === 1 && <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <CreatePortal
                        onCancel={this.navigateMap.bind(this, 0)} />
                </PersonaBarPage>
                }
                {props.selectedPage === 2 && <PersonaBarPage isOpen={props.selectedPage === 2}>
                    <ExportPortal
                        onCancel={this.cancelExport.bind(this)} />
                </PersonaBarPage>}
            </div>
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
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        portalBeingExported: state.exportPortal.portalBeingExported
    };
}


export default connect(mapStateToProps)(App);
