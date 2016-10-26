import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import PageHierarchy from "./PageHierarchy/PageHierarchy";
import SocialPanelBody from "dnn-social-panel-body";
import PersonaBarPage from "dnn-persona-bar-page";
import {
    visiblePanel as VisiblePanelActions,
    pageActions as PageActions 
} from "../actions";
import PageSettings from "../containers/PageSettings";

class App extends Component {

    navigateMap(page) {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page));
    }

    onPageSettings(pageId) {
        const {props} = this;
        this.navigateMap(1);
        props.dispatch(PageActions.loadPage(pageId));
    }

    onSavePage() {
        const {props} = this;
        props.dispatch(PageActions.savePage(props.selectedDnnPage));
    }

    onChangePageField(key, value) {
        const {props} = this;
        props.dispatch(PageActions.changePageField(key, value));
    }

    render() {
        const {props} = this;
        return (
            <div className="pages-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <SocialPanelHeader title="Pages">
                    </SocialPanelHeader>
                    <SocialPanelBody>                        
                        <PageHierarchy onPageSettings={pageId => this.onPageSettings(pageId)} />
                    </SocialPanelBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <SocialPanelHeader title="Pane 2">
                    </SocialPanelHeader>
                    <SocialPanelBody>
                        {props.selectedDnnPage && 
                            <PageSettings selectedPage={props.selectedDnnPage} 
                                          onCancel={this.navigateMap.bind(this, 0)} 
                                          onSave={this.onSavePage.bind(this)}
                                          onChangeField={this.onChangePageField.bind(this)}/>
                        }
                    </SocialPanelBody>
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    selectedDnnPage: PropTypes.object
};

function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        selectedDnnPage: state.pages.selectedPage
    };
}

export default connect(mapStateToProps)(App);
