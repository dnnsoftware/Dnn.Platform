import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import PersonaBarPage from "dnn-persona-bar-page";
import {
    pageActions as PageActions,
    addPagesActions as AddPagesActions
} from "../actions";
import PageSettings from "./PageSettings/PageSettings";
import AddPages from "./AddPages/AddPages";
import Localization from "../localization";
import PageList from "./PageList/PageList";
import Button from "dnn-button";
import utils from "../utils";
import BackToMain from "./common/BackToMain/BackToMain";
import panels from "../constants/panels";

class App extends Component {

    componentDidMount() {
        const {props} = this;
        const viewName = utils.getViewName();
        if (viewName === "edit") {
            props.onLoadPage(utils.getCurrentPageId());
        }
    }

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
        props.onLoadPage(pageId);
    }

    onSavePage() {
        const {props} = this;
        props.onSavePage(props.selectedPage);
    }

    onAddPage() {
        const {props} = this;
        props.onAddPage();
    }

    onCancelSettings() {
        if (this.props.selectedPageDirty) {
            this.showCancelWithoutSavingDialog();
        }
        else {
            this.props.onCancelPage();
        }
    }

    onCancelAddPages() {
        this.props.onCancelAddMultiplePages();
    }

    showCancelWithoutSavingDialog() {
        const onConfirm = () =>  this.props.onCancelPage();
        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm);
    }

    onAddMultiplePage() {
        this.props.onLoadAddMultiplePages();
    }

    getPageTitle() {
        const {selectedPage} = this.props;
        return selectedPage.tabId === 0 ? 
                Localization.get("AddPage") : 
                Localization.get("PageSettings") + ": " + selectedPage.name;
    }

    getSettingsPage() {
        const {props} = this;
        const titleSettings = this.getPageTitle();
        const cancelAction = this.onCancelSettings.bind(this);
        const backToPages = <BackToMain onClick={cancelAction}/>;
        
        return (<PersonaBarPage isOpen={props.selectedView === panels.PAGE_SETTINGS_PANEL}>
                    <SocialPanelHeader title={titleSettings}>
                    </SocialPanelHeader>
                    <SocialPanelBody
                        workSpaceTrayOutside={true}
                        workSpaceTray={backToPages}
                        workSpaceTrayVisible={true}>
                        <PageSettings selectedPage={props.selectedPage}
                                      selectedPageErrors={props.selectedPageErrors}
                                      selectedPageDirty={props.selectedPageDirty} 
                                      onCancel={cancelAction} 
                                      onSave={this.onSavePage.bind(this)}
                                      onChangeField={props.onChangePageField}
                                      onPermissionsChanged={props.onPermissionsChanged}
                                      onChangePageType={props.onChangePageType}
                                      onDeletePageModule={props.onDeletePageModule}
                                      onToggleEditPageModule={props.onToggleEditPageModule}
                                      editingSettingModuleId={props.editingSettingModuleId}
                                      onCopyAppearanceToDescendantPages={props.onCopyAppearanceToDescendantPages} />
                    </SocialPanelBody>
                </PersonaBarPage>);
    }

    getAddPages() {
        const {props} = this;
        const backToPages = <BackToMain onClick={this.onCancelAddPages.bind(this)}/>;

        return (<PersonaBarPage isOpen={props.selectedView === 2}>
                    <SocialPanelHeader title={Localization.get("AddMultiplePages")}>
                    </SocialPanelHeader>
                    <SocialPanelBody
                        workSpaceTrayOutside={true}
                        workSpaceTray={backToPages}
                        workSpaceTrayVisible={true}>
                        <AddPages  
                            bulkPage={props.bulkPage}
                            onCancel={this.onCancelAddPages.bind(this)} 
                            onSave={props.onSaveMultiplePages}
                            onChangeField={props.onChangeAddMultiplePagesField} />
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
                {props.selectedView === panels.PAGE_SETTINGS_PANEL && props.selectedPage && 
                    this.getSettingsPage()
                }
                {props.selectedView === panels.ADD_MULTIPLE_PAGES_PANEL && 
                    this.getAddPages()
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
    selectedPageDirty: PropTypes.boolean,
    bulkPage: PropTypes.object,
    editingSettingModuleId: PropTypes.number,
    onCancelPage: PropTypes.func.isRequired,
    onSavePage: PropTypes.func.isRequired,
    onLoadPage: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onCancelAddMultiplePages: PropTypes.func.isRequired,
    onSaveMultiplePages: PropTypes.func.isRequired,
    onLoadAddMultiplePages: PropTypes.func.isRequired,
    onChangeAddMultiplePagesField: PropTypes.func.isRequired,
    onChangePageField: PropTypes.func.isRequired,
    onChangePageType: PropTypes.func.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired,
    onDeletePageModule: PropTypes.func.isRequired,
    onToggleEditPageModule: PropTypes.func.isRequired,
    onCopyAppearanceToDescendantPages: PropTypes.func.isRequired,
    error: PropTypes.object
};

function mapStateToProps(state) {
    return {
        selectedView: state.visiblePanel.selectedPage,
        selectedPage: state.pages.selectedPage,
        selectedPageErrors: state.pages.errors,
        selectedPageDirty: state.pages.dirtyPage,
        bulkPage: state.addPages.bulkPage,
        editingSettingModuleId: state.pages.editingSettingModuleId,
        error: state.errors.error
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onCancelPage: PageActions.cancelPage,
        onSavePage: PageActions.savePage,
        onLoadPage: PageActions.loadPage,
        onAddPage: PageActions.addPage,
        onSaveMultiplePages: AddPagesActions.addPages,
        onCancelAddMultiplePages: AddPagesActions.cancelAddMultiplePages,
        onLoadAddMultiplePages: AddPagesActions.loadAddMultiplePages,
        onChangeAddMultiplePagesField: AddPagesActions.changeAddMultiplePagesField, 
        onChangePageField: PageActions.changePageField,
        onChangePageType: PageActions.changePageType,
        onPermissionsChanged: PageActions.changePermissions,
        onDeletePageModule: PageActions.deletePageModule,
        onToggleEditPageModule: PageActions.toggleEditPageModule,
        onCopyAppearanceToDescendantPages: PageActions.copyAppearanceToDescendantPages
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(App);
