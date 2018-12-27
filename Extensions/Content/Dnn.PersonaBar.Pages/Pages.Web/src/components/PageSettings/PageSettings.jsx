import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
import PageDetails from "../PageDetails/PageDetails";
import PermissionGrid from "../PermissionGrid/PermissionGrid";
import { Button, DnnTabs } from "@dnnsoftware/dnn-react-common";
import styles from "./style.less";
import Modules from "../Modules/Modules";
import Seo from "../Seo/Seo";
import More from "../More/More";
import Appearance from "../Appearance/Appearance";
import PageTypeSelector from "../PageTypeSelector/PageTypeSelector";
import PageLocalization from "../PageLocalization/PageLocalization";
import securityService from "../../services/securityService";
import permissionTypes from "../../services/permissionTypes";

class PageSettings extends Component {

    constructor(){
        super();
        this.state = { selectedPageName: "" }
    }

    hasPageErrors() {
        const { selectedPageErrors } = this.props;
        return Object.keys(selectedPageErrors)
            .map(key => selectedPageErrors[key])
            .some(value => value);
    }

    getButtons() {
        const { selectedPage, onCancel, onSave, onDelete, selectedPageDirty } = this.props;

        const saveButtonText = selectedPage.tabId === 0 ?
            Localization.get("AddPage") : Localization.get("Save");
        const pageErrors = this.hasPageErrors();

        let buttons = [<Button
            key="cancelButton"
            type="secondary"
            onClick={onCancel}>
            {Localization.get("Cancel")}
        </Button>,
        <Button
            key="saveButton"
            type="primary"
            disabled={(!selectedPageDirty || pageErrors) ? true : false}
            onClick={onSave.bind(this)}>
            {saveButtonText}
        </Button>];

        if (selectedPage.tabId !== 0
            && !selectedPage.isspecial
            && securityService.userHasPermission(permissionTypes.DELETE_PAGE, selectedPage)) {
            buttons.unshift(<Button
                key="delete"
                type="secondary"
                onClick={onDelete.bind(this, selectedPage)} >
                {Localization.get("Delete")}
            </Button>);
        }
        return buttons;
    }

    getPageFooter(buttons) {
        const { selectedPageDirty } = this.props;
        return (
            <div className="buttons-box">
                {buttons}
                {selectedPageDirty &&
                    <div className="dirty-info">
                        {Localization.get("ChangesNotSaved")}
                    </div>
                }
            </div>
        );
    }

    getCopyAppearanceToDescendantPagesButton() {
        return <Button
            type="secondary"
            onClick={this.props.onCopyAppearanceToDescendantPages}>
            {Localization.get("CopyAppearanceToDescendantPages")}
        </Button>;
    }

    getCopyPermissionsToDescendantPagesButton() {
        return <Button
            type="secondary"
            onClick={this.props.onCopyPermissionsToDescendantPages}>
            {Localization.get("CopyPermissionsToDescendantPages")}
        </Button>;
    }

    onSelectParentPageId(parentPageId, parentPageName) {
        if (this.state.parentPageId !== parentPageId) {
            this.setState({ parentPageId, parentPageName });
            this.props.onChangeParentId(parentPageId);
            this.props.onChangeField("hierarchy", parentPageName);
        }
    }
    
    render() {
        const {
            selectedPage,
            selectedPageErrors,
            onChangeField,
            onChangePageType,
            onDeletePageModule,
            onEditingPageModule,
            onCancelEditingPageModule,
            editingSettingModuleId,
            pageDetailsFooterComponents,
            pageTypeSelectorComponents,
            AllowContentLocalization,
            onGetCachedPageCount,
            onClearCache,
            onModuleCopyChange
        } = this.props;

        const buttons = this.getButtons();
        const isEditingExistingPage = selectedPage.tabId !== 0;
        const appearanceButtons = [...buttons];
        const permissionsButtons = [...buttons];

        const isLocalizationTabVisible = AllowContentLocalization && selectedPage.tabId !== 0;

        if (isEditingExistingPage && selectedPage.hasChild) {
            appearanceButtons.unshift(this.getCopyAppearanceToDescendantPagesButton());
            permissionsButtons.unshift(this.getCopyPermissionsToDescendantPagesButton());
        }

        const footer = this.getPageFooter(buttons);
        const appearanceFooter = this.getPageFooter(appearanceButtons);
        const permissionFooter = this.getPageFooter(permissionsButtons);

        const advancedTabs = [
            {
                label: Localization.get("Appearance"),
                component: <div className="dnn-simple-tab-item" key="appearance">
                    <Appearance page={selectedPage}
                        onChangeField={onChangeField} />
                    {appearanceFooter}
                </div>
            },
            {
                label: Localization.get("SEO"),
                component: <div className="dnn-simple-tab-item" key="seo">
                    <Seo page={selectedPage}
                        onChangeField={onChangeField} />
                    {footer}
                </div>
            },
            {
                label: Localization.get("More"),
                component: <div className="dnn-simple-tab-item" key="more">
                    <More page={selectedPage}
                        errors={selectedPageErrors}
                        onChangeField={onChangeField}
                        onGetCachedPageCount={onGetCachedPageCount}
                        onClearCache={onClearCache} />
                    {footer}
                </div>
            }
        ];

        if ((isEditingExistingPage || selectedPage.templateTabId) && selectedPage.pageType === "normal" && selectedPage.modules) {
            advancedTabs.unshift({
                label: Localization.get("Modules"),
                component: <div className="dnn-simple-tab-item dnn-simple-tab-item-modules" key="modules">
                    <Modules
                        modules={selectedPage.modules}
                        onDeleteModule={onDeletePageModule}
                        onEditingModule={onEditingPageModule}
                        onCancelEditingModule={onCancelEditingPageModule}
                        editingSettingModuleId={editingSettingModuleId} 
                        onModuleCopyChange={onModuleCopyChange}
                        selectedPage={selectedPage}
                        showCopySettings={!isEditingExistingPage && selectedPage.templateTabId} />
                    {footer}
                </div>
            });
        }

        let headers = [];
        let tabs = [];
        if (!isEditingExistingPage || securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            
            headers.push(Localization.get("Details"));
            tabs.push(
                <div className="dnn-simple-tab-item" key="details">
                    <PageTypeSelector
                        onChangePageType={onChangePageType}
                        components={pageTypeSelectorComponents} />
                    <PageDetails
                        selectedParentPageName={this.props.selectedPage.hierarchy}
                        selectedParentPageId={this.props.selectedPage.tabId}
                        onSelectParentPageId={this.onSelectParentPageId.bind(this)}
                        errors={selectedPageErrors}
                        onChangeField={onChangeField}
                        components={pageDetailsFooterComponents}
                    />
                    {footer}
                </div>);
        }
        if (!isEditingExistingPage ||securityService.userHasPermission(permissionTypes.ADMIN_PAGE, selectedPage)) {
            headers.push(Localization.get("Permissions"));
            if (isLocalizationTabVisible) {
                headers.push(Localization.get("Localization"));
            }
            tabs.push(<div className="dnn-simple-tab-item permission-tab" key="permissions">
                <PermissionGrid
                    permissions={selectedPage.permissions}
                    onPermissionsChanged={this.props.onPermissionsChanged} />
                {permissionFooter}
            </div>);
            if (isLocalizationTabVisible) {
                tabs.push(<div className="dnn-simple-tab-item dnn-simple-tab-item-localization" key="localization">
                    <PageLocalization
                        page={selectedPage}
                    />
                </div>);
            }
        }
        if (!isEditingExistingPage ||securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            headers.push(Localization.get("Advanced"));
            tabs.push(<div key="advanced">
                <DnnTabs
                    tabHeaders={advancedTabs.map(tab => tab.label)}
                    type="secondary">
                    {advancedTabs.map(tab => tab.component)}
                </DnnTabs>
            </div>);
        }
        return (
            <div>
                <DnnTabs
                    tabHeaders={headers}
                    className={styles.pageSettings}
                    onSelect={this.props.selectPageSettingTab.bind(this)}
                    selectedIndex={this.props.selectedPageSettingTab}>
                    {tabs}
                </DnnTabs>
            </div>
        );
    }
}

PageSettings.propTypes = {
    selectedPage: PropTypes.object.isRequired,
    selectedPageErrors: PropTypes.object.isRequired,
    selectedPageDirty: PropTypes.bool.isRequired,
    onCancel: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onDelete: PropTypes.func.isRequired,
    onChangeField: PropTypes.func.isRequired,
    onChangeParentId: PropTypes.func.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired,
    onChangePageType: PropTypes.func.isRequired,
    onDeletePageModule: PropTypes.func.isRequired,
    onEditingPageModule: PropTypes.func.isRequired,
    onCancelEditingPageModule: PropTypes.func.isRequired,
    onCopyAppearanceToDescendantPages: PropTypes.func.isRequired,
    onCopyPermissionsToDescendantPages: PropTypes.func.isRequired,
    editingSettingModuleId: PropTypes.number,
    pageDetailsFooterComponents: PropTypes.array.isRequired,
    pageTypeSelectorComponents: PropTypes.array.isRequired,
    selectedPageSettingTab: PropTypes.number,
    AllowContentLocalization: PropTypes.bool,
    selectPageSettingTab: PropTypes.func,
    onGetCachedPageCount: PropTypes.func.isRequired,
    onClearCache: PropTypes.func.isRequired,
    onModuleCopyChange: PropTypes.func
};

export default PageSettings;

