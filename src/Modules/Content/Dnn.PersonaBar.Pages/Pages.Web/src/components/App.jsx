import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
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
import Button from "dnn-button";

class App extends Component {

    navigateMap(page) {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page));
    }

    onPageSettings(pageId) {
        const {props} = this;
        props.onNavigate(1);
        props.onLoadPage(pageId);
    }

    onSavePage() {
        const {props} = this;
        props.onNavigate(0);
        props.onSavePage(props.selectedDnnPage);
    }

    onPermissionsChanged(permissions) {
        console.log("onPermissionsChanged", permissions);
        this.props.dispatch(PageActions.changePermissions(permissions));
    }

    onAddPage() {
        const {props} = this;
        props.onNavigate(1);
        props.onAddPage();
    }

    onAddMultiplePage() {
        
    }

    render() {
        const {props} = this;
        return (
            <div className="pages-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <SocialPanelHeader title={Localization.get("Pages")}>
                        <Button type="primary" size="large" onClick={this.onAddPage.bind(this)}>{Localization.get("Add Page") }</Button>
                        <Button type="secondary" size="large" onClick={this.onAddMultiplePage.bind(this)}>{Localization.get("Add Multiple Page") }</Button>
                    </SocialPanelHeader>
                    <PageList onPageSettings={this.onPageSettings.bind(this)} />
                </PersonaBarPage>
                {props.selectedDnnPage && 
                    <PersonaBarPage isOpen={props.selectedPage === 1}>
                        <SocialPanelHeader title={Localization.get("Page Settings:") + " " + props.selectedDnnPage.name}>
                        </SocialPanelHeader>
                        <SocialPanelBody>
                            <PageSettings selectedPage={props.selectedDnnPage} 
                                            onCancel={() => props.onNavigate(0)} 
                                            onSave={this.onSavePage.bind(this)}
                                            onChangeField={props.onChangePageField}
                                            onPermissionsChanged={this.onPermissionsChanged.bind(this)}
                                            onChangePageType={props.onChangePageType} />
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
    selectedDnnPage: PropTypes.object,
    onNavigate: PropTypes.func,
    onSavePage: PropTypes.func,
    onLoadPage: PropTypes.func,
    onAddPage: PropTypes.func,
    onChangePageField: PropTypes.func,
    onChangePageType: PropTypes.func
};

function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        selectedDnnPage: state.pages.selectedPage
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onNavigate: VisiblePanelActions.selectPanel,
        onSavePage: PageActions.savePage,
        onLoadPage: PageActions.loadPage,
        onAddPage: PageActions.addPage,
        onChangePageField: PageActions.changePageField,
        onChangePageType: PageActions.changePageType
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(App);
