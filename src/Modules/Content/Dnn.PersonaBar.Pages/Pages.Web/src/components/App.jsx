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
import utils from "../utils";

class App extends Component {

    componentWillReceiveProps(newProps) {
        this.notifyErrorIfNeeded(newProps);
    }

    notifyErrorIfNeeded(newProps) {
        if (newProps.error !== this.props.error) {
            const errorMessage = (newProps.error && newProps.error.message) || Localization.get("AnErrorOccurred");
            utils.notifyError(errorMessage);
        }
    }

    onPageSettings(pageId) {
        const {props} = this;
        props.onNavigate(1);
        props.onLoadPage(pageId);
    }

    onSavePage() {
        const {props} = this;
        props.onNavigate(0);
        props.onSavePage(props.selectedPage);
    }

    onAddPage() {
        const {props} = this;
        props.onNavigate(1);
        props.onAddPage();
    }

    onAddMultiplePage() {
        
    }

    getSettingsPage() {
        const {props} = this;
        const titleSettings = props.selectedPage.tabId === 0 ? Localization.get("AddPage") : Localization.get("PageSettings") + ": " + props.selectedPage.name;

        return (<PersonaBarPage isOpen={props.selectedView === 1}>
                    <SocialPanelHeader title={titleSettings}>
                    </SocialPanelHeader>
                    <SocialPanelBody>
                        <PageSettings selectedPage={props.selectedPage}
                                      selectedPageErrors={props.selectedPageErrors} 
                                      onCancel={() => props.onNavigate(0)} 
                                      onSave={this.onSavePage.bind(this)}
                                      onChangeField={props.onChangePageField}
                                      onPermissionsChanged={props.onPermissionsChanged}
                                      onChangePageType={props.onChangePageType}
                                      onDeletePageModule={props.onDeletePageModule}
                                      onToggleEditPageModule={props.onToggleEditPageModule}
                                      editingSettingModuleId={props.editingSettingModuleId} />
                    </SocialPanelBody>
                </PersonaBarPage>);
    }

    render() {
        const {props} = this;
        
        return (
            <div className="pages-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedView === 0}>
                    <SocialPanelHeader title={Localization.get("Pages")}>
                        <Button type="primary" size="large" onClick={this.onAddPage.bind(this)}>{Localization.get("AddPage") }</Button>
                        <Button type="secondary" size="large" onClick={this.onAddMultiplePage.bind(this)}>{Localization.get("AddMultiplePages") }</Button>
                    </SocialPanelHeader>
                    <PageList onPageSettings={this.onPageSettings.bind(this)} />
                </PersonaBarPage>
                {props.selectedPage && 
                    this.getSettingsPage()
                }
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedView: PropTypes.number,
    selectedPage: PropTypes.object,
    selectedPageErrors: PropTypes.object,
    editingSettingModuleId: PropTypes.number,
    onNavigate: PropTypes.func,
    onSavePage: PropTypes.func,
    onLoadPage: PropTypes.func,
    onAddPage: PropTypes.func,
    onChangePageField: PropTypes.func,
    onChangePageType: PropTypes.func,
    onPermissionsChanged: PropTypes.func.isRequired,
    onDeletePageModule: PropTypes.func.isRequired,
    onToggleEditPageModule: PropTypes.func.isRequired,
    error: PropTypes.object
};

function mapStateToProps(state) {
    return {
        selectedView: state.visiblePanel.selectedPage,
        selectedPage: state.pages.selectedPage,
        selectedPageErrors: state.pages.errors,
        editingSettingModuleId: state.pages.editingSettingModuleId,
        error: state.errors.error
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onNavigate: VisiblePanelActions.selectPanel,
        onSavePage: PageActions.savePage,
        onLoadPage: PageActions.loadPage,
        onAddPage: PageActions.addPage,
        onChangePageField: PageActions.changePageField,
        onChangePageType: PageActions.changePageType,
        onPermissionsChanged: PageActions.changePermissions,
        onDeletePageModule: PageActions.deletePageModule,
        onToggleEditPageModule: PageActions.toggleEditPageModule
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(App);
