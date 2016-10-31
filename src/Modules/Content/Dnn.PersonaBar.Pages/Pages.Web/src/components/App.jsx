import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import PersonaBarPage from "dnn-persona-bar-page";
import {
    visiblePanel as VisiblePanelActions,
    pageActions as PageActions
} from "../actions";
import PageSettings from "./PageSettings/PageSettings";
import Localization from "../localization";
import PageList from "./PageList/PageList";

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

    onPermissionsChanged(permissions) {
        console.log("onPermissionsChanged", permissions);
        this.props.dispatch(PageActions.changePermissions(permissions));
    }
    
    render() {
        const {props} = this;
        return (
            <div className="pages-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <SocialPanelHeader title={Localization.get("Pages")} />
                    <PageList />
                </PersonaBarPage>
                {props.selectedDnnPage && 
                    <PersonaBarPage isOpen={props.selectedPage === 1}>
                        <SocialPanelHeader title={Localization.get("Page Settings:") + " " + props.selectedDnnPage.name}>
                        </SocialPanelHeader>
                        <SocialPanelBody>
                            <PageSettings selectedPage={props.selectedDnnPage} 
                                            onCancel={this.navigateMap.bind(this, 0)} 
                                            onSave={this.onSavePage.bind(this)}
                                            onChangeField={this.onChangePageField.bind(this)}
                                            onPermissionsChanged={this.onPermissionsChanged.bind(this)} />
                        </SocialPanelBody>
                    </PersonaBarPage>
                }
            </div>
        );
    }
}

App.propTypes = {
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
