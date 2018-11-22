import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Button from "dnn-button";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import VocabularyList from "./VocabularyList";
import CreateVocabulary from "./CreateVocabulary";
import PersonaBarPage from "dnn-persona-bar-page";
import { visiblePanel as VisiblePanelActions } from "../actions";
import LocalizedResources from "../resources";
import util from "utils";

class App extends Component {
    constructor() {
        super();
    }
    openCreateVocabulary() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(1));
    }
    closeCreateVocabulary() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(0));
    }
    navigateMap(page, index, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
    }

    render() {
        const {props} = this;
        return (
            <div className="taxonomy-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={LocalizedResources.get("ControlTitle_")}>
                        {util.canEdit() && <Button type="primary" size="large" onClick={this.openCreateVocabulary.bind(this)}>{LocalizedResources.get("Create")}</Button>}
                    </PersonaBarPageHeader>
                    <VocabularyList />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <PersonaBarPageHeader
                        title={LocalizedResources.get("Create")}
                        onCreateVocabulary={this.openCreateVocabulary.bind(this)}
                    />
                    {props.selectedPage === 1 && <CreateVocabulary onCloseVocabulary={this.closeCreateVocabulary.bind(this)} isOpen={props.selectedPage === 1} />}
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number
};


function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);