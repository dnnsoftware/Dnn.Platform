import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import VocabularyList from "./VocabularyList";
import CreateVocabulary from "./CreateVocabulary";
import PersonaBarPage from "dnn-persona-bar-page";
import {visiblePanel as VisiblePanelActions } from "../actions";
import LocalizedResources from "../resources";
require("es6-object-assign").polyfill();
require("array.prototype.find").shim();
require("array.prototype.findindex").shim();

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
                    <SocialPanelHeader title={LocalizedResources.get("ControlTitle_") }>
                        <Button type="primary" size="large" onClick={this.openCreateVocabulary.bind(this) }>{LocalizedResources.get("Create") }</Button>
                    </SocialPanelHeader>
                    <VocabularyList />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <SocialPanelHeader
                        title={LocalizedResources.get("Create") }
                        onCreateVocabulary={this.openCreateVocabulary.bind(this) }
                        />
                    <CreateVocabulary onCloseVocabulary={this.closeCreateVocabulary.bind(this) } isOpen={props.selectedPage === 1}/>
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