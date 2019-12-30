
import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { PersonaBarPageHeader, PersonaBarPage, Button, GridCell } from "@dnnsoftware/dnn-react-common";
import {
    pageActions as PageActions,
    addPagesActions as AddPagesActions,
    templateActions as TemplateActions,
    visiblePanelActions as VisiblePanelActions,
    visiblePageSettingsActions as VisiblePageSettingsActions,
    languagesActions as LanguagesActions,
    pageHierarchyActions as PageHierarchyActions
} from "../actions";
import PageSettings from "./PageSettings/PageSettings";
import AddPages from "./AddPages/AddPages";
import Localization from "../localization";
import PageList from "./PageList/PageList";
import SaveAsTemplate from "./SaveAsTemplate/SaveAsTemplate";
import utils from "../utils";
import panels from "../constants/panels";
import Sec from "./Security/Sec";
import securityService from "../services/securityService";
import permissionTypes from "../services/permissionTypes";
import BreadCrumbs from "./BreadCrumbs";
import Promise from "promise";
import SearchPageInput from "./SearchPage/SearchPageInput";

import "./style.less";

import {PersonaBarPageTreeviewInteractor} from "./dnn-persona-bar-page-treeview";
import SearchResult from "./SearchPage/SearchResult";

function getSelectedTabBeingViewed(viewTab) {
    switch (viewTab) {
        case "details":
            return 0;
        case "permissions":
            return 1;
        case "localization":
            return 2;
        case "advanced":
            return 3;
    }
}


class App extends Component {
    constructor() {
        super();
        const date = new Date();
        this.state = {
            referral: "",
            referralText: "",
            busy: false,
            headerDropdownSelection: "Save Page Template",
            
            inSearch: false,
            searchTerm: "",
            filtersUpdated: false,

            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,

            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],

            tags: "",
            filters: [],
            pageIndex:0,
            searchFields: {},

            inDuplicateMode: false
        };
        this.lastActivePageId = null;
        this.shouldRunRecursive = true;
        this.noPermissionSelectionPageId = null;
    }

    componentDidMount() {
        let { selectedPage } = this.props;
        if (securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            this.props.getContentLocalizationEnabled();
        }

        const viewName = utils.getViewName();
        const viewParams = utils.getViewParams();
        window.dnn.utility.setConfirmationDialogPosition();
        window.dnn.utility.closeSocialTasks();
        this.props.getPageList().then(() => {
            const selectedPageId = utils.getSelectedPageId() || utils.getCurrentPageId();
            selectedPageId && !utils.getIsAdminHostSystemPage() && this.onLoadPage(selectedPageId);
        
            if (viewName === "edit") {
                this.onLoadPage(utils.getCurrentPageId());
            }

            if (!utils.isPlatform()) {
                this.props.getWorkflowsList();
            }

            //Resolve tab being viewed, if view params are present.
            this.resolveTabBeingViewed(viewParams);

            //Listen to event fired to view page settings (from site settings)
            document.addEventListener("viewPageSettings", this.resolveTabBeingViewed.bind(this), false);
        });
    }

    //Update referral text if coming from a referral. (ex: "SiteSettings", resx.get("BackToLanguages"))
    updateReferral(referral, referralText) {
        this.setState({
            referral,
            referralText
        });
    }

    //Method to go back to referral panel.
    goToReferralPanel(referral) {
        let personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel(referral, {}); //Open, panel should already be rendered, so no need to pass params.
            this.updateReferral("", "");
        }
    }

    //Call method to go back to referral panel
    backToReferral(referral) {
        this.goToReferralPanel(referral);
    }

    //Resolves tab being viewed if view params are present.
    resolveTabBeingViewed(viewParams) {

        if (!viewParams) {
            return;
        }
        if (viewParams.pageId) {
            this.onLoadPage(viewParams.pageId);

        }
        if (viewParams.viewTab) {
            this.selectPageSettingTab(getSelectedTabBeingViewed(viewParams.viewTab));
        }
        if (viewParams.referral) {
            this.updateReferral(viewParams.referral, viewParams.referralText);
        }
    }
    
    componentWillUnmount() {
        document.removeEventListener("viewPageSettings", this.resolveTabBeingViewed);
    }

    componentDidUpdate(prevProps) {
        if (this.props.error && this.props.error && this.props.error !== prevProps.error){
            const errorMessage = (this.props.error && this.props.error.message) || Localization.get("AnErrorOccurred");
            utils.notify(errorMessage);
        }
        window.dnn.utility.closeSocialTasks();
        const { selectedPage } = this.props;
        if (selectedPage && selectedPage.tabId > 0 && selectedPage.canManagePage !== undefined && !selectedPage.canManagePage) {
            this.noPermissionSelectionPageId = utils.getCurrentPageId();
            this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
        }
        if (selectedPage && selectedPage.tabId > 0 && this.shouldRunRecursive) {
            this.shouldRunRecursive = false;
            this.buildTree(selectedPage.tabId);
        }
    }

    buildTree(selectedId) {
        const buildTreeInternal = (hierarchy) => {
            const callAPI = () => {
                const parentId = hierarchy.shift();
                parentId && setTimeout(() => execute(), 100);
                const execute = () => {
                    if (parentId !== selectedId) {
                        let page = null;
                        this._traverse((item, list, update) => {
                            if (item.id === parentId) {
                                item.isOpen = true;
                                item.hasChildren = true;
                                page = item;
                                update(list);
                                return;
                            }
                        });
                        if (!page || !page.childListItems) {
                            this.props.getChildPageList(parentId)
                                .then(data => {
                                    this._traverse((item, list, update) => {
                                        const left = () => {
                                            item.childListItems = data;
                                            item.isOpen = true;
                                            item.hasChildren = true;
                                            update(list);
                                            callAPI();
                                        };
                                        const right = () => { update(list); };
                                        (data.length > 0 && item.id === data[0].parentId) ? left() : right();
                                    });
                                });
                        } else {
                            callAPI();
                        }
                    } else {
                        this.buildBreadCrumbPath(selectedId);
                    }
                };
            };
            callAPI();
        };
        this.props.getPageHierarchy(selectedId).then(buildTreeInternal);
    }

    onPageSettings(pageId) {
        this.onLoadPage(pageId);
    }

    onCreatePage() {
        this.props.onCreatePage((page) => {
            if (page && page.canAddContentToPage || page && page.pageType !== "normal" || utils.getIsSuperUser()) {
                page.selected = true;
                if (page.parentId && page.parentId !== -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === page.parentId) {
                            switch (true) {
                                case item.childCount > 0 && !item.childListItems:
                                    this.props.getChildPageList(item.id).then((data) => {
                                        item.isOpen = true;
                                        data.map(child => {
                                            if (child.id === page.id) {
                                                child.selected = true;
                                            }
                                            return child;
                                        });
                                        item.childListItems = data;
                                        updateStore(list);
                                        if (page.canAddContentToPage)
                                            this.onLoadPage(page.id);
                                        else {
                                            this.onNoPermissionSelection(page.id);
                                        }
                                    });
                                    break;
                                case item.childCount === 0 && !item.childListItems:
                                    item.childCount++;
                                    item.childListItems = [];
                                    item.childListItems.push(page);
                                    if (page.canAddContentToPage)
                                        this.onLoadPage(page.id);
                                    else {
                                        this.onNoPermissionSelection(page.id);
                                    }
                                    break;
                                case Array.isArray(item.childListItems) === true: {
                                        const lastIndex = item.childListItems.length - 1;
                                        if (item.childListItems[lastIndex].name === page.name) {
                                            item.childListItems.pop();
                                        }
                                        item.childListItems.push(page);
                                        item.childCount = item.childListItems.length;
                                        if (page.canAddContentToPage)
                                            this.onLoadPage(page.id);
                                        else {
                                            this.onNoPermissionSelection(page.id);
                                        }
                                        break;
                                    }
                            }
                            item.isOpen = true;
                            updateStore(list);
                        }
                    });
                }
                else {
                    this.props.getPageList().then(() => {
                        this._traverse((item, list, updateStore) => {
                            if (item.id === page.id) {
                                item.isOpen = true;
                                item.selected = true;
                                updateStore(list);
                                if (page.canAddContentToPage)
                                    this.onLoadPage(page.id);
                                else {
                                    this.onNoPermissionSelection(page.id);
                                }
                            }
                        });
                    });
                }
            }
            else {
                let self = this;
                self.setEmptyStateMessage();
            }
        });
    }

    onUpdatePage(input) {
        this.shouldRunRecursive = false;
        return new Promise((resolve) => {
            const update = (input && input.tabId) ? input : this.props.selectedPage;
            let cachedItem = null;

            const removeFromOldParent = () => {
                const left = () => {
                    const { pageList } = this.props;
                    pageList.forEach((item, index) => {
                        if (item.id === update.tabId) {
                            cachedItem = item;
                            const arr1 = pageList.slice(0, index);
                            const arr2 = pageList.slice(index + 1);
                            const newPageList = [...arr1, ...arr2];
                            this.props.updatePageListStore(newPageList);
                        }
                    });
                };


                const right = () => {

                    this._traverse((item, list, updateStore) => {
                        if (item.id === update.oldParentId) {
                            item.childListItems.forEach((child, index) => {
                                if (child.id === update.tabId) {
                                    cachedItem = child;
                                    const arr1 = item.childListItems.slice(0, index);
                                    const arr2 = item.childListItems.slice(index + 1);
                                    item.childListItems = [...arr1, ...arr2];
                                    item.childCount--;
                                    updateStore(list);
                                }
                            });
                        }
                    });
                };

                (update.oldParentId === -1 || update.parentId === -1) ? left() : right();
            };

            const addToNewParent = () => {
                if (update.parentId === -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === list[list.length - 1].id) {
                            (cachedItem) ? cachedItem.parentId = -1 : null;
                            const listCopy = [...list, cachedItem];
                            updateStore(listCopy);
                        }
                    });
                } else {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === update.parentId) {

                            (cachedItem) ? cachedItem.parentId = item.id : null;

                            switch (true) {
                                case item.childCount > 0 && !item.childListItems:
                                    this.props.getChildPageList(item.id).then((data) => {
                                        item.isOpen = true;
                                        item.childListItems = data;
                                        updateStore(list);
                                    });
                                    break;
                                case item.childCount === 0 && !item.childListItems:
                                    item.childCount++;
                                    item.childListItems = [];
                                    item.childListItems.push(cachedItem);
                                    break;
                                case Array.isArray(item.childListItems) === true:
                                    item.childCount++;
                                    item.childListItems.push(cachedItem);
                                    this.onLoadPage(cachedItem.id);
                                    break;
                            }
                            item.isOpen = true;
                            updateStore(list);
                        }
                    });
                }
            };

            this.props.onUpdatePage(update, (page) => {
                if (update.oldParentId) {
                    if (page.id === utils.getCurrentPageId()) {
                        window.parent.location = page.url;
                    }
                    else {
                        removeFromOldParent();
                        addToNewParent();
                    }
                }

                this._traverse((item, list, updateStore) => {
                    if (item.id === update.tabId) {
                        item.name = page.name;
                        item.pageType = page.pageType;
                        item.url = page.url;
                        item.includeInMenu = update.includeInMenu;
                        updateStore(list);
                    }
                });
                this.buildTree(update.tabId);
                resolve();
                this.setState({inDuplicateMode: false});
            });
        });
    }

    
    onLoadPage(pageId, callback) {
        const self = this;
        this.props.onLoadPage(pageId).then((data) => {
            self.buildBreadCrumbPath(data.tabId);
            if (typeof callback === "function")
                callback(data);
        });
    }

    _addPageToTree(parentId) {
        let runUpdateStore = null;
        let pageList = null;
        
        if(parentId !== null && parentId !== -1) {
            this._traverse((item, list, updateStore) => {
                if (item.id === parentId) {
                    item.isOpen = true;
                    item.hasChildren = true;
                    item.childCount++;
                    const newPageChildItems = item.childListItems.concat(this.props.selectedPage);
                    item.childListItems = newPageChildItems;
                    updateStore(list);        
                }
            });

        } else {
            this._traverse((item, list, updateStore) => {
                item.selected = false;
                runUpdateStore = updateStore;
                pageList = list;                
            });
            const newPageList = pageList.concat(this.props.selectedPage);    
            runUpdateStore(newPageList);
        }
    }

    _removePageFromTree(parentId) {
        this._traverse((item, list, updateStore) => {
            if (item.id === parentId && parentId !== undefined) {
                let itemIndex = null;
                item.childCount--;
                (item.childCount === 0) ? item.isOpen = false : null;

                item.childListItems.forEach((child, index) => {
                    if (child.tabId !== undefined) {
                        itemIndex = index;
                    }
                });
                const arr1 = item.childListItems.slice(0, itemIndex);
                const arr2 = item.childListItems.slice(itemIndex + 1);
                item.childListItems = [...arr1, ...arr2];
                updateStore(list);
            }
            if ((parentId === undefined || parentId === -1) && item.tabId !== undefined) {
                const newPageList = list.slice(0,list.length-1);
                updateStore(newPageList);
            }
        });


    }

    onCancelPage(parentPageId) {
        this._removePageFromTree(parentPageId);
        this.props.changeSelectedPagePath([]);
        (parentPageId !== -1) ? this.props.onCancelPage(parentPageId) : this.props.onCancelPage();
        this.setState({inDuplicateMode: false});
    }

    onChangeParentId() {
        this.onChangePageField('oldParentId', this.props.selectedPage.parentId);
    }

    onAddMultiplePage() {
        this._testDirtyPage(()=>{
            this.clearEmptyStateMessage();
            this.selectPageSettingTab(0);
            this.props.clearSelectedPage();
    
            this.props.onLoadAddMultiplePages();
        });  
        
    }
    
    /**
     * When on edit mode
     */
    onEditMode() {
        const {selectedPage, selectedView, selectedPageDirty} = this.props;
        return ((selectedPage && selectedPage.tabId === 0) || selectedPageDirty
            || selectedView === panels.SAVE_AS_TEMPLATE_PANEL
            || selectedView === panels.ADD_MULTIPLE_PAGES_PANEL 
            || selectedView === panels.CUSTOM_PAGE_DETAIL_PANEL);
    }

    onAddPage(parentPage) {
        this.clearEmptyStateMessage();
        this.selectPageSettingTab(0);
        
        const addPage = () => {
            const { props } = this;
            const { selectedPage } = props;
            let runUpdateStore = null;
            let pageList = null;
        
            this._traverse((item, list, updateStore) => {
                item.selected = false;
                pageList = list;
                runUpdateStore = updateStore;
            });
            runUpdateStore(pageList);

            const onConfirm = () => {
                this.props.changeSelectedPagePath([]);
                
                this.props.getNewPage(parentPage).then(()=>{
                    if (parentPage && parentPage.id) {
                        this.props.getChildPageList(parentPage.id)
                        .then(pageChildItems => {
                            this._traverse((item, list, update) => {
                                const updateChildItems = () => {
                                    const newPageChildItems = pageChildItems.concat(this.props.selectedPage);
                                    item.childListItems = newPageChildItems;
                                    item.isOpen = true;
                                    item.hasChildren = true;
                                    update(list);
                                };
                                (item.id === parentPage.id) ? updateChildItems() : null;
                            });
                        });
                    } else {
                        this._traverse((item, list, updateStore) => {
                            pageList = list;
                            runUpdateStore = updateStore;
                        });
                        const newPageList = pageList.concat(this.props.selectedPage);    
                        runUpdateStore(newPageList);
                    }

                    this.setState({inDuplicateMode: false});
                }); 
            };
       

            if (selectedPage && selectedPage.tabId !== 0 && props.selectedPageDirty) {
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                onConfirm();
            }
            
            setTimeout(()=>{
                if (this.node.querySelector("#name")) {
                    this.node.querySelector("#name").focus();
                }    
            },100);
        };

        const noPermission = () => this.setEmptyStateMessage("You do not have permission to add a child page to this parent");
        parentPage.canAddPage === undefined || parentPage.canAddPage ? addPage() : noPermission();
    }

    onCancelSettings() {
        const { props } = this;
        if (props.selectedPageDirty) {
            
            this.showCancelWithoutSavingDialog();
        }
        else {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
            }
            else if (props.selectedPage.tabId === 0 && props.selectedPage.referralTabId) {
                this.onCancelPage(props.selectedPage.referralTabId);
            }
            else {
                this.onCancelPage();
            }
        }
    }

    onDeleteSettings() {
        const { props } = this;
        const { selectedPage } = props;

        const left = () => {
            return () => {
                this._traverse((item, list, updateStore) => {
                    if (item.id === props.selectedPage.parentId) {
                        let itemIndex = null;
                        item.childCount--;
                        (item.childCount === 0) ? item.isOpen = false : null;

                        item.childListItems.forEach((child, index) => {
                            if (child.id === props.selectedPage.tabId) {
                                itemIndex = index;
                            }
                        });
                        const arr1 = item.childListItems.slice(0, itemIndex);
                        const arr2 = item.childListItems.slice(itemIndex + 1);
                        item.childListItems = [...arr1, ...arr2];
                        props.onDeletePage(props.selectedPage, false, utils.getCurrentPageId() === props.selectedPage.tabId ? item.url : null);
                        updateStore(list);
                        this.onCancelPage();
                    }
                });
            };
        };

        const right = () => {
            return () => {
                let itemIndex;
                const pageList = JSON.parse(JSON.stringify(this.props.pageList));
                pageList.forEach((item, index) => {
                    if (item.id === selectedPage.tabId) {
                        itemIndex = index;
                    }
                });

                const arr1 = pageList.slice(0, itemIndex);
                const arr2 = pageList.slice(itemIndex + 1);
                const update = [...arr1, ...arr2];
                this.props.onDeletePage(props.selectedPage, false, utils.getCurrentPageId() === props.selectedPage.tabId ? utils.getDefaultPageUrl() : null);
                this.props.updatePageListStore(update);
                this.onCancelPage();
            };
        };

        const onDelete = (selectedPage.parentId !== -1) ? left() : right();

        utils.confirm(
            Localization.get("DeletePageConfirm"),
            Localization.get("Delete"),
            Localization.get("Cancel"),
            onDelete);
    }

    onClearCache() {
        const { props } = this;
        props.onClearCache();
    }

    showCancelWithoutSavingDialog() {
        const { props } = this;
        const onConfirm = () => {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
            }
            else if (props.selectedPage.tabId === 0 && props.selectedPage.referralTabId) {
                this.onCancelPage(props.selectedPage.referralTabId);
            }
            else {
                this.onCancelPage();
            }
        };

        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm);
    }


    showCancelWithoutSavingDialogInEditMode(input) {
        const id = (typeof input === "object") ? this.props.selectedPage.tabId : input;
        if (this.props.selectedPageDirty) {
            const onConfirm = () => {
                this.onLoadPage(id, () => {
                    this._traverse((item, list) => {
                        if (item.id === id) {
                            Object.keys(this.props.selectedPage).forEach((key) => item[key] = this.props.selectedPage[key]);
                            this.props.updatePageListStore(list);
                            this.selectPageSettingTab(0);
                            this.lastActivePageId = null;
                        }
                    });
                });
            };


            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Continue"),
                Localization.get("Go Back"),
                onConfirm);

        } else {
            this.onCancelPage();
            this.props.changeSelectedPagePath([]);

        }
    }

    isNewPage() {
        const { selectedPage } = this.props;
        return selectedPage.tabId === 0;
    }

    getPageTitle() {
        const { selectedPage } = this.props;
        return this.isNewPage() ?
            Localization.get("AddPage") :
            selectedPage.name;
    }
    
    _testDirtyPage(callback) {
        const {selectedPage, selectedPageDirty} = this.props;
        const onConfirm = () => {
            callback();
            this.onLoadPage(selectedPage.tabId, () => {
                this._traverse((item, list) => {
                    if (item.id === selectedPage.tabId) {
                        Object.keys(this.props.selectedPage).forEach((key) => item[key] = this.props.selectedPage[key]);
                        this.props.updatePageListStore(list);
                        this.selectPageSettingTab(0);
                        this.lastActivePageId = null;
                    }
                });
            });
        };

        if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);

        } else {
            return onConfirm();
        }
    }

    onLoadSavePageAsTemplate() {
        this._testDirtyPage(this.props.onLoadSavePageAsTemplate.bind(this));
    }

    getSettingsButtons() {
        const { selectedPage, settingsButtonComponents, onLoadSavePageAsTemplate, onShowPageSettings, onHidePageSettings } = this.props;
        const SaveAsTemplateButton = settingsButtonComponents.SaveAsTemplateButton || Button;
        
        return (
            <div className="heading-buttons">
                <Sec permission={permissionTypes.EXPORT_PAGE} selectedPage={selectedPage}>
                    <SaveAsTemplateButton
                        type="secondary"
                        size="large"
                        disabled={this.onEditMode() || this.state.inSearch}
                        onClick={this.onLoadSavePageAsTemplate.bind(this)}
                        onShowPageSettingsCallback={onShowPageSettings}
                        onHidePageSettings={onHidePageSettings}
                        onSaveAsPlatformTemplate={onLoadSavePageAsTemplate}>
                        {Localization.get("SaveAsTemplate")}
                    </SaveAsTemplateButton>
                </Sec>

            </div>
        );
    }

    selectPageSettingTab(index) {
        this.props.selectPageSettingTab(index);
    }

    onSaveMultiplePages(){    
        return this.props.onSaveMultiplePages(()=>{
            this.props.getPageList();
        });
    }
    
    onValidateMultiplePages(){    
        return this.props.onValidateMultiplePages(()=>{
            // stay on same page
        });
    }
    
    onCancelAddMultiplePages(){
        const { props } = this;
        
        if (props.dirtyBulkPage) {
            const onConfirm = () => {
                props.onCancelAddMultiplePages();
            };

            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);
        } else {
            props.onCancelAddMultiplePages();
        }    
    }

    getAddPages() {
        const { props } = this;
        return (
                <AddPages
                    bulkPage={props.bulkPage}
                    onCancel={this.onCancelAddMultiplePages.bind(this)}
                    onValidate={this.onValidateMultiplePages.bind(this)}
                    onSave={this.onSaveMultiplePages.bind(this)}
                    onChangeField={props.onChangeAddMultiplePagesField}
                    components={props.multiplePagesComponents} />);
    }

    onCancelSaveCustomDetail(onCancelSave) {
        return ((isDirty) =>{
            if (isDirty) {
                const onConfirm = () => {
                    onCancelSave();
                };

                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);
            } else {
                onCancelSave();
            }
        });
    }

    onChangeCustomDetail() {

    }
    
    onCancelSavePageAsTemplate() {
        const { props } = this;
        
        if (props.dirtyTemplate) {
            const onConfirm = () => {
                props.onCancelSavePageAsTemplate();
            };

            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);
        } else {
            props.onCancelSavePageAsTemplate();
        }
    }

    getSaveAsTemplatePage() {
        
        return (
                <SaveAsTemplate
                    onCancel={this.onCancelSavePageAsTemplate.bind(this)} />);
    }


    _traverse(comparator, pageListCopy) {
        let listItems = pageListCopy || JSON.parse(JSON.stringify(this.props.pageList));
        const cachedChildListItems = [];
        cachedChildListItems.push(listItems);
        const condition = cachedChildListItems.length > 0;

        const loop = () => {
            const childItem = cachedChildListItems.length ? cachedChildListItems.shift() : null;
            const left = () => childItem.forEach(item => {
                comparator(item, listItems, (pageList) => this.props.updatePageListStore(pageList));
                Array.isArray(item.childListItems) ? cachedChildListItems.push(item.childListItems) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childItem ? left() : right();
        };

        const exit = () => null;

        loop();
        return;
    }

    setActivePage(pageInfo) {
        return new Promise((resolve) => {
            this.selectPageSettingTab(0);
            pageInfo.id = pageInfo.id || pageInfo.tabId;
            pageInfo.tabId = pageInfo.tabId || pageInfo.id;
            this.onLoadPage(pageInfo.tabId);
            resolve();
        });
    }

    onSelection(pageId) {
        const { selectedPage, selectedPageDirty } = this.props;
        this.selectPageSettingTab(0);
        this.shouldRunRecursive = false;
        const left = () => {
            if (!selectedPage || selectedPage.tabId !== pageId) {
                this.onLoadPage(pageId);
                this.selectPageSettingTab(0);
            }
        };
        const right = () => (!selectedPage || pageId !== selectedPage.tabId) ? this.showCancelWithoutSavingDialogInEditMode(pageId) : null;
        (!selectedPageDirty) ? left() : right();
    }

    onNoPermissionSelection(pageId) {
        const setNoPermissionState = () => {
            this.onCancelPage();
            this.buildBreadCrumbPath(pageId);
            this.noPermissionSelectionPageId = pageId;
            this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
        };
        if (this.props.selectedPageDirty) {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Continue"),
                Localization.get("Go Back"),
                setNoPermissionState);
        }
        else {
            setNoPermissionState();
        }
    }

    onChangePageField(key, value) {
        if (this.props.selectedPage[key] !== value) {
            this.props.onChangePageField(key, value);
        }
        this.updatePageNameOnList(key, value);
    }

    updatePageNameOnList(key, value) {
        this._traverse((item, list, updateStore) => {
            if ((item.tabId === 0 || item.id === this.props.selectedPage.tabId) && key === "name") {
                item.name = value;
                item.selected = true;
                item.isOpen = true;
                updateStore(list);
            } 
        });
    }

    onMovePage({ Action, PageId, ParentId, RelatedPageId }) {
        return PageActions.movePage({ Action, PageId, ParentId, RelatedPageId });
    }

    CallCustomAction(action) {
        const { selectedPage, selectedPageDirty } = this.props;
        const callAction = () => {
            if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
                const onConfirm = () => {
                    action();
                };
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                action();
            }
        };
        callAction();
    }

    onDuplicatePage(item) {
        const { selectedPage, selectedPageDirty } = this.props;
        const message = Localization.get("NoPermissionCopyPage");
        const duplicate = () => {
            if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
                const onConfirm = () => {
                    this.props.onDuplicatePage(true);
                };
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                this.props.onDuplicatePage(false).then(data=>{
                    
                    this._addPageToTree(data.parentId);
                });
            }
            this.setState({inDuplicateMode: true});
        };
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canCopyPage ? duplicate() : noPermission();
    }

    onViewEditPage(item) {
        const { selectedPageDirty } = this.props;
        const viewPage = () => PageActions.viewPage(item.id, item.url);

        const showConfirmationDialog = () => {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                viewPage);
        };

        const proceed = () => selectedPageDirty ? showConfirmationDialog() : viewPage();

        this.clearEmptyStateMessage();
        const message = Localization.get("NoPermissionEditPage");
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canAddContentToPage ? proceed() : noPermission();

    }

    onViewPage(item) {
        const { selectedPageDirty } = this.props;
        const view = () => {
            //this.onLoadPage(item.id);
            utils.getUtilities().closePersonaBar(function () {
                window.parent.location = item.url;
            });
        };

        const left = () => {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                view);
        };

        const right = () => view();
        const proceed = () => selectedPageDirty ? left() : right();

        this.clearEmptyStateMessage();
        const message = Localization.get("NoPermissionViewPage");
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canViewPage ? proceed() : noPermission();

    }

    setEmptyStateMessage(emptyStateMessage) {
        this.setState({ emptyStateMessage });
        this.props.clearSelectedPage();
    }

    clearEmptyStateMessage() {
        this.noPermissionSelectionPageId = null;
        this.setState({ emptyStateMessage: null });
    }
    
    onDayClick(newDay, isEndDate) {
        this.setState({ startAndEndDateDirty: true });
        const right = () => {
            const condition = newDay.getTime() < this.state.endDate.getTime();
            condition ? this.setState({ startDate: newDay, filtersUpdated: true }) : this.setState({ startDate: newDay, endDate: newDay, filtersUpdated: true });
        };

        const left = () => {
            const condition = newDay.getTime() >= this.state.startDate.getTime();
            condition ? this.setState({ endDate: newDay, filtersUpdated: true }) : null;
        };
        isEndDate ? left() : right();
    }

    generateFilters() {
        const { filterByPageType, filterByPublishStatus, filterByWorkflow, filterByWorkflowName, startDate, endDate, startAndEndDateDirty } = this.state;
        const filters = this.state.tags.split(",")
            .filter(e => !!e)
            .map((tag) => {
                return { ref: `tag-${tag}`, tag: `${tag}` };
            });

        filterByPageType ? filters.push({ ref: "filterByPageType", tag: `${Localization.get("PageType")}: ${this.getPageTypeLabel(filterByPageType)}` }) : null;
        filterByPublishStatus ? filters.push({ ref: "filterByPublishStatus", tag: `${Localization.get("lblPublishStatus")}: ${this.getPublishStatusLabel(filterByPublishStatus)}` }) : null;
        filterByWorkflow ? filters.push({ ref: "filterByWorkflow", tag: `${Localization.get("WorkflowTitle")}: ${filterByWorkflowName}` }) : null;
        
        if (startAndEndDateDirty) {
            let dateRangeText = Localization.get(utils.isPlatform() ? "ModifiedDateRange" : "PublishedDateRange");
            const fullStartDate = utils.formatDate(startDate);
            const fullEndDate = utils.formatDate(endDate);
            const dateInterval = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate} - ${fullEndDate} ` });
            const justOneDate = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate}` });

            fullStartDate !== fullEndDate ? dateInterval() : justOneDate();
        }
    }

    saveSearchFilters(searchFields) {
        return new Promise((resolve) => this.setState({ searchFields }, () => resolve()));
    }

    
    onSearch(term) {
        let newTerm = (term !== undefined) ? term : this.state.searchTerm;
        this.setState({
            searchTerm: newTerm,
            filtersUpdated: true,
            pageIndex:0
        },()=>{
            const { selectedPage } = this.props;
            const { filtersUpdated } = this.state;
            if (filtersUpdated) {
                if (selectedPage) {
                    this.lastActivePageId = selectedPage.tabId;
                    if (this.props.selectedPageDirty) {
                        this.showCancelWithoutSavingDialogAndRun(() => {
                            this.doSearch();
                        }, () => {
                            this.clearSearch();
                        });
                    } else {
                        this.doSearch();
                    }
                }
                else {
                    this.doSearch();
                }          
            }
            this.setState({ DropdownCalendarIsActive: null});
        });
    }

    doSearch() {
        const { selectedPage } = this.props;
        if (selectedPage && this.props.selectedPageDirty) {
            this.onCancelPage(selectedPage.tabId);
        }

        let { searchTerm, filterByPageType, filterByPublishStatus, filterByWorkflow, startDate, endDate, startAndEndDateDirty, tags } = this.state;
        const fullStartDate = `${startDate.getDate() < 10 ? `0` + startDate.getDate() : startDate.getDate()}/${((startDate.getMonth() + 1) < 10 ? `0` + (startDate.getMonth() + 1) : (startDate.getMonth() + 1))}/${startDate.getFullYear()} 00:00:00`;
        const fullEndDate = `${endDate.getDate() < 10 ? `0` + endDate.getDate() : endDate.getDate()}/${((endDate.getMonth() + 1) < 10 ? `0` + (endDate.getMonth() + 1) : (endDate.getMonth() + 1))}/${endDate.getFullYear()} 23:59:59`;
        const searchDateRange = startAndEndDateDirty ? { publishDateStart: fullStartDate, publishDateEnd: fullEndDate } : {};

        if (tags) {
            tags = tags[0] === "," ? tags.replace(",", "") : tags;
            tags = tags[tags.length - 1] === "," ? tags.split(",").filter(t => !!t).join() : tags;
        }

        let search = { 
            tags: tags, 
            searchKey: searchTerm, 
            pageType: filterByPageType, 
            publishStatus: filterByPublishStatus, 
            workflowId: filterByWorkflow,
            pageSize: 10 
        };
        
        search = Object.assign({}, search, searchDateRange);
        for (let prop in search) {
            if (!search[prop]) {
                delete search[prop];
            }
        }
        
        search = Object.assign({},search,{pageIndex:this.state.pageIndex});
        
        this.generateFilters();
        const filterUp = this.state.filtersUpdated;
        const filterPage = this.state.pageIndex;
        
        this.saveSearchFilters(search).then(() => {
            this.props.searchAndFilterPagedPageList(search,filterUp,filterPage);
        });        
        this.setState({ 
            inSearch: true, 
            filtersUpdated: false 
        });
    }

    onSearchScroll(page) {
        this.setState({
            pageIndex: page
        },()=>{
            this.doSearch();
        });
    }
    
    clearAdvancedSearch() {
        let date = new Date();
        this.setState({
            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,
            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],
            tags: "",
            filters: [],
            searchFields: {},
            filtersUpdated: true,
            pageIndex:0
        },()=>this.onSearch(this.state.searchTerm));
    }

    clearAdvancedSearchDateInterval(){
        let date = new Date();
        this.setState({
            startDate:date,
            endDate: date,
            startAndEndDateDirty:false
        });
    }
    
    clearSearch(callback) {
        let date = new Date();
        this.setState({
            filtersUpdated: true,
            pageIndex:0,
            inSearch: false,
            searchTerm: "",
            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,
            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],
            tags: "",
            filters: [],
            searchFields: {}
        }, () => {
            if (typeof callback === "function") {
                callback();
            }
            else {
                const { selectedPage } = this.props;
                !selectedPage && this.lastActivePageId && this.onLoadPage(this.lastActivePageId);
            }
        });
    }
    showCancelWithoutSavingDialogAndRun(callback, cancelCallback) {

        const onConfirm = () => {
            if (typeof callback === "function") {
                callback();
            }
        };
        const onCancel = () => {
            if (typeof cancelCallback === "function") {
                cancelCallback();
            }
        };
        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm,
            onCancel);
    }
    buildBreadCrumbPath(pageId) {
        let page = {};
        let selectedPath = [];
        const buildBreadCrumbPathInternal = () => {
            const addNode = (tabId) => {
                this._traverse((item) => {
                    if (item.id === tabId) {
                        page = item;
                        return;
                    }
                });
                
                selectedPath.push({ name: page.name, tabId: (page.id !== pageId ? page.id : null) });
                const left = () => {
                    addNode(page.parentId);
                };
                const right = () => {
                    selectedPath.reverse();
                    this.props.changeSelectedPagePath(selectedPath);
                };
                page.parentId > 0 ? left() : right();
            };
            addNode(pageId);
        };
        buildBreadCrumbPathInternal();
    }

    render_PagesDetailEditor() {

        const render_emptyState = () => {
            const DefaultMessage = Localization.get("NoPageSelected");
            return (
                <div className="empty-page-state">
                    <div className="empty-page-state-message">
                        <h1>{this.state.emptyStateMessage || DefaultMessage}</h1>
                        <p>Select a page in the tree to manage its settings here.</p>
                    </div>
                </div>
            );
        };


        const render_pageDetails = () => {
            const { props } = this;
            const { isContentLocalizationEnabled } = props;
            return (
                <PageSettings key={"pageDetails" + this.props.selectedPage.tabId}
                    selectedPage={this.props.selectedPage}
                    AllowContentLocalization={isContentLocalizationEnabled}
                    selectedPageErrors={{}}
                    selectedPageDirty={props.selectedPageDirty}
                    onCancel={this.showCancelWithoutSavingDialogInEditMode.bind(this)}
                    onDelete={this.onDeleteSettings.bind(this)}
                    onSave={this.onUpdatePage.bind(this)}
                    selectedPageSettingTab={props.selectedPageSettingTab}
                    selectPageSettingTab={this.selectPageSettingTab.bind(this)}
                    onChangeField={this.onChangePageField.bind(this)}
                    onChangeParentId={this.onChangeParentId.bind(this)}
                    onPermissionsChanged={props.onPermissionsChanged}
                    onChangePageType={props.onChangePageType.bind(this)}
                    onDeletePageModule={props.onDeletePageModule}
                    onEditingPageModule={props.onEditingPageModule}
                    onCancelEditingPageModule={props.onCancelEditingPageModule}
                    editingSettingModuleId={props.editingSettingModuleId}
                    onCopyAppearanceToDescendantPages={props.onCopyAppearanceToDescendantPages}
                    onCopyPermissionsToDescendantPages={props.onCopyPermissionsToDescendantPages}
                    pageDetailsFooterComponents={props.pageDetailsFooterComponents}
                    pageTypeSelectorComponents={props.pageTypeSelectorComponents}
                    onGetCachedPageCount={props.onGetCachedPageCount}
                    onClearCache={props.onClearCache}
                    onModuleCopyChange={props.onModuleCopyChange}
                    customPageSettingsComponents={props.customPageSettingsComponents}
                />
            );
        };
        const { selectedPage } = this.props;
        return (
            <GridCell columnSize={100} className="treeview-page-details" >
                {(selectedPage && selectedPage.tabId) ? render_pageDetails() : render_emptyState()}
            </GridCell>
        );
    }

    render_addPageEditor() {
        const { props } = this;
        const cancelAction = this.onCancelSettings.bind(this);
        const deleteAction = this.onDeleteSettings.bind(this);
        const AllowContentLocalization = !!props.isContentLocalizationEnabled;
        return (
            <GridCell columnSize={100} className="treeview-page-details" >
                <PageSettings key="newPageSettings" selectedPage={props.selectedPage}
                    AllowContentLocalization={AllowContentLocalization}
                    selectedPageErrors={props.selectedPageErrors}
                    selectedPageDirty={props.selectedPageDirty}
                    onCancel={cancelAction}
                    onDelete={deleteAction}
                    onSave={this.onCreatePage.bind(this)}
                    selectedPageSettingTab={props.selectedPageSettingTab}
                    selectPageSettingTab={this.selectPageSettingTab.bind(this)}
                    onChangeField={this.onChangePageField.bind(this)}
                    onChangeParentId={this.onChangeParentId.bind(this)}
                    onPermissionsChanged={props.onPermissionsChanged}
                    onChangePageType={props.onChangePageType}
                    onDeletePageModule={props.onDeletePageModule}
                    onEditingPageModule={props.onEditingPageModule}
                    onCancelEditingPageModule={props.onCancelEditingPageModule}
                    editingSettingModuleId={props.editingSettingModuleId}
                    onCopyAppearanceToDescendantPages={props.onCopyAppearanceToDescendantPages}
                    onCopyPermissionsToDescendantPages={props.onCopyPermissionsToDescendantPages}
                    pageDetailsFooterComponents={props.pageDetailsFooterComponents}
                    pageTypeSelectorComponents={props.pageTypeSelectorComponents}
                    onGetCachedPageCount={props.onGetCachedPageCount}
                    onClearCache={props.onClearCache}
                    onModuleCopyChange={props.onModuleCopyChange} />
            </GridCell>
        );
    }


    render_pageList() {
        return (
            <PageList onPageSettings={this.onPageSettings.bind(this)} />
        );
    }
    
    getFilterByPageTypeOptions() {
        return [
            { value: "", label: Localization.get("lblNone") },
            { value: "normal", label: Localization.get("lblNormal") },
            { value: "tab", label: Localization.get("Existing") },
            { value: "url", label: Localization.get("lblUrl") },
            { value: "file", label: Localization.get("lblFile") }
        ];
    }

    getFilterByPageStatusOptions() {
        let filterByPageStatusOptions = [
            { value: "published", label: Localization.get("lblPublished") }
        ];
        if (!utils.isPlatform()) {
            filterByPageStatusOptions = ([{ value: "", label: Localization.get("lblNone") }]).concat(filterByPageStatusOptions.concat([{ value: "draft", label: Localization.get("lblDraft") }]));
        }
        return filterByPageStatusOptions;
    }
    getFilterByWorkflowOptions() {
        let workflowList = [];
        if (!utils.isPlatform() && this.props.workflowList.length <= 0) {
            this.props.getWorkflowsList();
        }
        this.props.workflowList.length ? workflowList = this.props.workflowList.map((item => { return { value: item.workflowId, label: item.workflowName }; })) : null;
        return [{ value: "", label: Localization.get("lblNone") }].concat(workflowList);
    }
    getPageTypeLabel(pageType) {
        const filterByPageTypeOptions = this.getFilterByPageTypeOptions();
        return filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()) && filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()).label;
    }
    getPublishStatusLabel(publishStatus) {
        const filterByPublishStatusOptions = this.getFilterByPageStatusOptions();
        return filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()) && filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()).label || publishStatus;
    }

    updateSearchAdvancedTags(tags) {
        this.setState({tags:tags,filtersUpdated:true, pageIndex:0});
    }

    onApplyChangesDropdownDayPicker() {
        const { startAndEndDateDirty, startDate, endDate } = this.state;
        const fullStartDate = startDate.getDate() + startDate.getMonth() + startDate.getFullYear();
        const fullEndDate = endDate.getDate() + endDate.getMonth() + endDate.getFullYear();
        const condition = !startAndEndDateDirty && fullStartDate === fullEndDate;
        condition ? this.setState({ startAndEndDateDirty: true, DropdownCalendarIsActive: null }) : this.setState({ DropdownCalendarIsActive: null });
    }
    
    updateFilterByPageTypeOptions(data) { 
        this.setState({ filterByPageType: data.value, filtersUpdated: true,pageIndex:0 }); 
    }
    
    updateFilterByPageStatusOptions(data) {
        this.setState({ filterByPublishStatus: data.value, filtersUpdated: true, pageIndex:0 });
    } 
    updateFilterByWorkflowOptions(data) {
        this.setState({ filterByWorkflow: data.value, filterByWorkflowName: data.label, filtersUpdated: true, pageIndex:0});
    }

    updateFilterStartEndDate(startDate, endDate) {
        this.setState({
            startDate,endDate,
            startAndEndDateDirty:true,
            filtersUpdated:true,
            pageIndex:0
        });
    }

    render_searchResults() {
        return (
            <SearchResult 
                filters={this.state.filters} 
                tags={this.state.tags}
                filterPageByType={this.state.filterByPageType}
                filterByPublishStatus={this.state.filterByPublishStatus}
                filtersUpdated={this.state.filtersUpdated}
                startDate={this.state.startDate} 
                endDate={this.state.endDate}
                getPageTypeLabel={this.getPageTypeLabel.bind(this)}
                getPublishStatusLabel={this.getPublishStatusLabel.bind(this)}
                getFilterByPageTypeOptions={this.getFilterByPageTypeOptions.bind(this)}
                getFilterByPageStatusOptions={this.getFilterByPageStatusOptions.bind(this)}
                getFilterByWorkflowOptions={this.getFilterByWorkflowOptions.bind(this)}
                filterByWorkflow={this.state.filterByWorkflow}
                onApplyChangesDropdownDayPicker={this.onApplyChangesDropdownDayPicker.bind(this)}
                updateFilterByPageTypeOptions={this.updateFilterByPageTypeOptions.bind(this)}
                updateFilterByPageStatusOptions={this.updateFilterByPageStatusOptions.bind(this)}
                updateFilterByWorkflowOptions={this.updateFilterByWorkflowOptions.bind(this)}
                filterByPageType={this.state.filterByPageType}
                onDayClick={this.onDayClick.bind(this)}
                startAndEndDateDirty={this.state.startAndEndDateDirty}
                onSearch={this.onSearch.bind(this)}
                clearSearch={this.clearSearch.bind(this)}
                clearAdvancedSearch={this.clearAdvancedSearch.bind(this)}
                clearAdvancedSearchDateInterval={this.clearAdvancedSearchDateInterval.bind(this)}
                buildBreadCrumbPath={this.buildBreadCrumbPath.bind(this)} 
                setEmptyStateMessage={this.setEmptyStateMessage.bind(this)}
                onViewPage={this.onViewPage.bind(this)}
                onViewEditPage={this.onViewEditPage.bind(this)}
                CallCustomAction={this.CallCustomAction.bind(this)}
                onLoadPage={this.onLoadPage.bind(this)}
                updateSearchAdvancedTags={this.updateSearchAdvancedTags.bind(this)}
                updateFilterStartEndDate={this.updateFilterStartEndDate.bind(this)}
                buildTree={this.buildTree.bind(this)}
                onSearchScroll={this.onSearchScroll.bind(this)}
                pageIndex={this.state.pageIndex}
            />
        );
    }
    
    getAdditionalPageSettings() {
        const additionalPageSettings = [];

        const { props } = this;
        
        if (props.customPageSettingsComponents) {
            for (let i = 0; i < props.customPageSettingsComponents.length; i++) {
                const customPageSettings = props.customPageSettingsComponents[i];
                if (props.selectedCustomPageSettings.pageSettingsId === customPageSettings.panelId) {
                    const Component = customPageSettings.component;
                    additionalPageSettings.push(
                        <Component
                            onCancel={this.onCancelSaveCustomDetail(this.props.onCancelSavePageAsTemplate)} 
                            onChange={this.props.onChangeCustomDetails}
                            selectedPage={props.selectedPage}
                            disabled={this.onEditMode()}
                            store={customPageSettings.store} />
                    );
                }
            }
        }

        return additionalPageSettings;
    }
    
    render_details() {
        const {selectedPage} = this.props;
        const {inSearch} = this.state;
        const {selectedView} = this.props;

        switch (true) {
            case inSearch:
                return this.render_searchResults();
            case selectedPage && selectedPage.tabId === 0:
                return this.render_addPageEditor();
            case selectedView === panels.ADD_MULTIPLE_PAGES_PANEL:
                return this.getAddPages();
            case selectedView === panels.SAVE_AS_TEMPLATE_PANEL:
                return this.getSaveAsTemplatePage();
            case selectedView === panels.CUSTOM_PAGE_DETAIL_PANEL:
                return this.getAdditionalPageSettings();
            case !selectedPage:
            default:
                return this.render_PagesDetailEditor();
        }
    }

    render() {
        const { props } = this;
        const { selectedPage } = props;
        const { inSearch } = this.state;
        const isListPagesAllowed = securityService.canSeePagesList();
       
         /* eslint-disable react/no-danger */
        return (

            <div ref={node => this.node = node} className="pages-app personaBar-mainContainer">
                { isListPagesAllowed &&
                    <PersonaBarPage fullWidth={true} isOpen={true}>
                        <PersonaBarPageHeader title={Localization.get(inSearch ? "PagesSearchHeader" : "Pages")}>
                            {securityService.isSuperUser() &&
                                <div>
                                    <Button type="primary" disabled={ this.onEditMode()  || this.state.inSearch} size="large" onClick={this.onAddPage.bind(this)}>{Localization.get("AddPage")}</Button>
                                    <Button type="secondary" disabled={ this.onEditMode() || this.state.inSearch } size="large" onClick={this.onAddMultiplePage.bind(this)}>{Localization.get("AddMultiplePages")}</Button>
                                </div>
                            }
                            { 
                                selectedPage && this.getSettingsButtons()
                            }                            
                            {!inSearch && <BreadCrumbs items={this.props.selectedPagePath || []} onSelectedItem={this.onSelection.bind(this)} />}
                        </PersonaBarPageHeader>
                        <GridCell columnSize={100} style={{ padding: "30px 30px 16px 30px" }}>
                            <SearchPageInput 
                                inSearch={this.state.inSearch} 
                                onSearch={this.onSearch.bind(this)}
                                clearSearch={this.clearSearch.bind(this)}  />
                        </GridCell>
                        <GridCell columnSize={100} style={{ padding: "0px 30px 30px 30px" }} >
                            <GridCell columnSize={1096} type={"px"} className="page-container">
                                <div className={(this.onEditMode()|| this.state.inSearch) ? "tree-container disabled" : "tree-container"}>
                                    <PersonaBarPageTreeviewInteractor
                                        clearSelectedPage={this.props.clearSelectedPage}
                                        Localization={Localization}
                                        pageList={this.props.pageList}
                                        getChildPageList={this.props.getChildPageList}
                                        getPage={this.props.getPage.bind(this)}
                                        _traverse={this._traverse.bind(this)}
                                        showCancelDialog={this.showCancelWithoutSavingDialogInEditMode.bind(this)}
                                        selectedPageDirty={this.props.selectedPageDirty}
                                        setEmptyPageMessage={this.setEmptyStateMessage.bind(this)}
                                        setActivePage={this.setActivePage.bind(this)}
                                        saveDropState={this.onUpdatePage.bind(this)}
                                        onMovePage={this.onMovePage.bind(this)}
                                        onViewPage={this.onViewPage.bind(this)}
                                        onViewEditPage={this.onViewEditPage.bind(this)}
                                        onDuplicatePage={this.onDuplicatePage.bind(this)}
                                        onAddPage={this.onAddPage.bind(this)}
                                        onSelection={this.onSelection.bind(this)}
                                        onNoPermissionSelection={this.onNoPermissionSelection.bind(this)}
                                        pageInContextComponents={props.pageInContextComponents}
                                        NoPermissionSelectionPageId={this.noPermissionSelectionPageId}
                                        CallCustomAction={this.CallCustomAction.bind(this)}
                                        enabled={!((selectedPage && selectedPage.tabId === 0) || inSearch)} />
                                </div>
                                <GridCell columnSize={760} type={"px"}>
                                    {this.render_details()}
                                </GridCell>
                            </GridCell>
                        </GridCell>
                    </PersonaBarPage>
                }
               
            </div>
        );
    }
}

App.propTypes = {
    pageList: PropTypes.array.isRequired,
    searchPageList: PropTypes.func.isRequired,
    searchAndFilterPagedPageList: PropTypes.func.isRequired,
    getChildPageList: PropTypes.func.isRequired,
    getWorkflowsList: PropTypes.func.isRequired,
    selectedView: PropTypes.number,
    selectedPage: PropTypes.object,
    selectedPageErrors: PropTypes.object,
    selectedPageDirty: PropTypes.bool,
    selectedTemplateDirty: PropTypes.bool,
    filtersUpdated: PropTypes.bool,
    bulkPage: PropTypes.object,
    dirtyBulkPage : PropTypes.bool, 
    editingSettingModuleId: PropTypes.number,
    onCancelPage: PropTypes.func.isRequired,
    onCreatePage: PropTypes.func.isRequired,
    onUpdatePage: PropTypes.func.isRequired,
    onDeletePage: PropTypes.func.isRequired,
    getPageList: PropTypes.func.isRequired,
    getPage: PropTypes.func.isRequired,
    updatePageListStore: PropTypes.func.isRequired,
    getNewPage: PropTypes.func.isRequired,
    onLoadPage: PropTypes.func.isRequired,
    onCancelAddMultiplePages: PropTypes.func.isRequired,
    onValidateMultiplePages: PropTypes.func.isRequired,
    onSaveMultiplePages: PropTypes.func.isRequired,
    onLoadAddMultiplePages: PropTypes.func.isRequired,
    onChangeAddMultiplePagesField: PropTypes.func.isRequired,
    onChangePageField: PropTypes.func.isRequired,
    onChangePageType: PropTypes.func.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired,
    onDeletePageModule: PropTypes.func.isRequired,
    onEditingPageModule: PropTypes.func.isRequired,
    onCancelEditingPageModule: PropTypes.func.isRequired,
    onCopyAppearanceToDescendantPages: PropTypes.func.isRequired,
    onCopyPermissionsToDescendantPages: PropTypes.func.isRequired,
    onLoadSavePageAsTemplate: PropTypes.func.isRequired,
    onCancelSavePageAsTemplate: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    error: PropTypes.object,
    multiplePagesComponents: PropTypes.array.isRequired,
    pageDetailsFooterComponents: PropTypes.array.isRequired,
    settingsButtonComponents: PropTypes.object.isRequired,
    pageTypeSelectorComponents: PropTypes.array.isRequired,
    pageInContextComponents: PropTypes.array.isRequired,
    selectedPageSettingTab: PropTypes.number,
    selectPageSettingTab: PropTypes.func,
    additionalPanels: PropTypes.array.isRequired,
    onShowPanel: PropTypes.func.isRequired,
    onHidePanel: PropTypes.func.isRequired,
    onShowPageSettings: PropTypes.func,
    onHidePageSettings: PropTypes.func,
    isContentLocalizationEnabled: PropTypes.bool,
    getContentLocalizationEnabled: PropTypes.func.isRequired,
    selectPage: PropTypes.func.isRequired,
    selectedPagePath: PropTypes.array.isRequired,
    changeSelectedPagePath: PropTypes.func.isRequired,
    onGetCachedPageCount: PropTypes.func,
    onClearCache: PropTypes.func.isRequired,
    clearSelectedPage: PropTypes.func.isRequired,
    onModuleCopyChange: PropTypes.func,
    workflowList: PropTypes.array.isRequired,
    customPageSettingsComponents: PropTypes.array,
    getPageHierarchy: PropTypes.func.isRequired,
    dirtyTemplate: PropTypes.bool,
    dirtyCustomDetails: PropTypes.bool,
    onChangeCustomDetails: PropTypes.func
};

function mapStateToProps(state) {
    return {
        pageList: state.pageList.pageList,
        selectedView: state.visiblePanel.selectedPage,
        selectedCustomPageSettings : state.visiblePageSettings.panelId,
        selectedPage: state.pages.selectedPage,
        selectedPageErrors: state.pages.errors,
        selectedPageDirty: state.pages.dirtyPage,
        filtersUpdated: state.pages.filtersUpdated,
        dirtyTemplate: state.template.dirtyTemplate,
        dirtyEvoqTemplate: state.template.dirtyEvoqTemplate,
        bulkPage: state.addPages.bulkPage,
        dirtyBulkPage : state.addPages.dirtyBulkPage,
        editingSettingModuleId: state.pages.editingSettingModuleId,
        error: state.errors.error,
        multiplePagesComponents: state.extensions.multiplePagesComponents,
        pageDetailsFooterComponents: state.extensions.pageDetailsFooterComponents,
        pageInContextComponents: state.extensions.pageInContextComponents,
        settingsButtonComponents: state.extensions.settingsButtonComponents,
        pageTypeSelectorComponents: state.extensions.pageTypeSelectorComponents,
        selectedPageSettingTab: state.pages.selectedPageSettingTab,
        additionalPanels: state.extensions.additionalPanels,
        isContentLocalizationEnabled: state.languages.isContentLocalizationEnabled,
        selectedPagePath: state.pageHierarchy.selectedPagePath,
        workflowList: state.pages.workflowList,
        customPageSettingsComponents : state.extensions.pageSettingsComponent,
        dirtyCustomDetails: state.pages.dirtyCustomDetails
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators({
        getNewPage: PageActions.getNewPage,
        getPageList: PageActions.getPageList,
        searchPageList: PageActions.searchPageList,
        searchAndFilterPagedPageList: PageActions.searchAndFilterPagedPageList,
        getWorkflowsList: PageActions.getWorkflowsList,
        getPage: PageActions.getPage,
        viewPage: PageActions.viewPage,
        getChildPageList: PageActions.getChildPageList,
        updatePageListStore: PageActions.updatePageListStore,
        onCancelPage: PageActions.cancelPage,
        onCreatePage: PageActions.createPage,
        onUpdatePage: PageActions.updatePage,
        onDeletePage: PageActions.deletePage,
        selectPageSettingTab: PageActions.selectPageSettingTab,
        onLoadPage: PageActions.loadPage,
        onSaveMultiplePages: AddPagesActions.addPages,
        onValidateMultiplePages: AddPagesActions.validatePages,
        onCancelAddMultiplePages: AddPagesActions.cancelAddMultiplePages,
        onLoadAddMultiplePages: AddPagesActions.loadAddMultiplePages,
        onChangeAddMultiplePagesField: AddPagesActions.changeAddMultiplePagesField,
        onChangePageField: PageActions.changePageField,
        onChangePageType: PageActions.changePageType,
        onPermissionsChanged: PageActions.changePermissions,
        onDeletePageModule: PageActions.deletePageModule,
        onEditingPageModule: PageActions.editingPageModule,
        onCancelEditingPageModule: PageActions.cancelEditingPageModule,
        onCopyAppearanceToDescendantPages: PageActions.copyAppearanceToDescendantPages,
        onCopyPermissionsToDescendantPages: PageActions.copyPermissionsToDescendantPages,
        onLoadSavePageAsTemplate: TemplateActions.loadSavePageAsTemplate,
        onCancelSavePageAsTemplate: TemplateActions.cancelSavePageAsTemplate,
        onDuplicatePage: PageActions.duplicatePage,
        onShowPanel: VisiblePanelActions.showPanel, 
        onHidePanel: VisiblePanelActions.hidePanel,
        onShowPageSettings: VisiblePageSettingsActions.showCustomPageSettings,
        onHidePageSettings: VisiblePageSettingsActions.hideCustomPageSettings,
        getContentLocalizationEnabled: LanguagesActions.getContentLocalizationEnabled,
        selectPage: PageHierarchyActions.selectPage,
        changeSelectedPagePath: PageHierarchyActions.changeSelectedPagePath,
        onGetCachedPageCount: PageActions.getCachedPageCount,
        onClearCache: PageActions.clearCache,
        clearSelectedPage: PageActions.clearSelectedPage,
        onModuleCopyChange: PageActions.updatePageModuleCopy,
        getPageHierarchy: PageActions.getPageHierarchy,
        onChangeCustomDetails: PageActions.dirtyCustomDetails

    }, dispatch);
}
App.contextTypes = {
    scrollArea: PropTypes.object
};
export default connect(mapStateToProps, mapDispatchToProps)(App);