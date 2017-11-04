
import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPage from "dnn-persona-bar-page";
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
import Button from "dnn-button";
import utils from "../utils";
import panels from "../constants/panels";
import Sec from "./Security/Sec";
import securityService from "../services/securityService";
import permissionTypes from "../services/permissionTypes";
import BreadCrumbs from "./BreadCrumbs";
import cloneDeep from "lodash/clonedeep";
import GridCell from "dnn-grid-cell";
import OverflowText from "dnn-text-overflow-wrapper";
import Promise from "promise";

import { PagesSearchIcon, PagesVerticalMore, CalendarIcon, ArrowBack, EyeIcon, TreeEdit, TreeAnalytics } from "dnn-svg-icons";
import Dropdown from "dnn-dropdown";
import { XIcon } from "dnn-svg-icons";

import "./style.less";

import DropdownDayPicker from "./DropdownDayPicker/DropdownDayPicker";

import { PersonaBarPageTreeviewInteractor } from "./dnn-persona-bar-page-treeview";

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
            toggleSearchMoreFlyout: false,
            DropdownCalendarIsActive: null,

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
            searchFields: {}
        };
        this.lastActivePageId = null;
        this.shouldRunRecursive = true;
        this.noPermissionSelectionPageId = null;
    }

    componentDidMount() {
        const viewName = utils.getViewName();
        const viewParams = utils.getViewParams();
        window.dnn.utility.setConfirmationDialogPosition();
        window.dnn.utility.closeSocialTasks();
        this.props.getPageList();
        const selectedPageId = utils.getCurrentPageId();
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

    componentWillMount() {
        let { selectedPage } = this.props;
        if (securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            this.props.getContentLocalizationEnabled();
        }
    }

    componentWillUnmount() {
        document.removeEventListener("viewPageSettings");
    }


    componentWillReceiveProps(newProps) {
        this.notifyErrorIfNeeded(newProps);
        window.dnn.utility.closeSocialTasks();
        const { selectedPage } = newProps;
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
    notifyErrorIfNeeded(newProps) {
        if (newProps.error !== this.props.error) {
            const errorMessage = (newProps.error && newProps.error.message) || Localization.get("AnErrorOccurred");
            utils.notifyError(errorMessage);
        }
    }

    onPageSettings(pageId) {
        const { props } = this;
        this.onLoadPage(pageId);
    }

    onCreatePage() {
        this.props.onCreatePage((page) => {
            if (page && page.canAddContentToPage || utils.getIsSuperUser()) {
                page.selected = true;
                if (page.parentId && page.parentId !== -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === page.parentId) {
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
                                    item.childListItems.push(page);
                                    this.onLoadPage(page.id);
                                    break;
                                case Array.isArray(item.childListItems) === true:
                                    item.childCount++;
                                    item.childListItems.push(page);
                                    this.onLoadPage(page.id);
                                    break;
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
                                this.onLoadPage(page.id);
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
            let newList = null;
            let cachedItem = null;

            const removeFromOldParent = () => {
                const left = () => {
                    const { pageList } = this.props;
                    pageList.forEach((item, index) => {
                        if (item.id == update.tabId) {
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
                        if (item.id == update.oldParentId) {
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
                if (update.parentId == -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === list[list.length - 1].id) {
                            (cachedItem) ? cachedItem.parentId = -1 : null;
                            const listCopy = [...list, cachedItem];
                            updateStore(listCopy);
                        }
                    });
                } else {
                    this._traverse((item, list, updateStore) => {
                        if (item.id == update.parentId) {

                            (cachedItem) ? cachedItem.parentId = item.id : null;

                            switch (true) {
                                case item.childCount > 0 && !item.childListItems:
                                    this.props.getChildPageList(item.id).then((data) => {
                                        item.isOpen = true;
                                        item.childListItems = data;
                                        updateStore(list);
                                    });
                                    break;
                                case item.childCount == 0 && !item.childListItems:
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
                        updateStore(list);
                    }
                });
                this.buildTree(update.tabId);
                //this.onLoadPage(update.tabId);
                resolve();
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
    onCancelPage(pageId) {
        this.props.changeSelectedPagePath("");
        this.props.onCancelPage(pageId);
    }

    onChangeParentId(newParentId) {
        this.onChangePageField('oldParentId', this.props.selectedPage.parentId);
    }

    onSearchFocus() {

    }

    onSearchFieldChange(e) {
        let self = this;
        const currentSearchTerm = this.state.searchTerm;
        const inSearch = this.state.inSearch;
        this.setState({ searchTerm: e.target.value, filtersUpdated: true }, () => {
            const { searchTerm } = this.state;
            switch (true) {
                case searchTerm.length > 3:
                    self.onSearch();
                    return;
                case currentSearchTerm.length > 0 && searchTerm.length === 0 && inSearch:
                    self.onSearch();
                    return;
            }
        });
    }

    onAddMultiplePage() {
        this.clearEmptyStateMessage();
        this.selectPageSettingTab(0);

        this.props.onLoadAddMultiplePages();
        
    }
    /**
     * When on edit mode
     */
    onEditMode(){
        const {selectedPage, selectedView} = this.props;
        return (selectedPage && selectedPage.tabId === 0 
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
            const onConfirm = () => { this.props.changeSelectedPagePath(""); this.props.getNewPage(parentPage); };
            if (selectedPage && selectedPage.tabId !== 0 && props.selectedPageDirty) {

                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                onConfirm();
            }
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
                this.onCancelPage(props.selectedPage.templateTabId);
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
                        props.onDeletePage(props.selectedPage, utils.getCurrentPageId() === props.selectedPage.tabId ? item.url : null);
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
                this.props.onDeletePage(props.selectedPage, utils.getCurrentPageId() === props.selectedPage.tabId ? utils.getDefaultPageUrl() : null);
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
                this.onCancelPage(props.selectedPage.templateTabId);
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
                this.onLoadPage(id, (data) => {
                    this._traverse((item, list, updateStore) => {
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
            this.props.changeSelectedPagePath("");

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

    getSettingsButtons() {
        const { selectedPage, settingsButtonComponents, onLoadSavePageAsTemplate, onDuplicatePage, onShowPageSettings, onHidePageSettings } = this.props;
        const SaveAsTemplateButton = settingsButtonComponents.SaveAsTemplateButton || Button;
        
        return (
            <div className="heading-buttons">
                <Sec permission={permissionTypes.EXPORT_PAGE} selectedPage={selectedPage}>
                    <SaveAsTemplateButton
                        type="secondary"
                        size="large"
                        disabled={this.onEditMode()}
                        onClick={onLoadSavePageAsTemplate}
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
                this.props.onDuplicatePage(false);
            }
        };
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canCopyPage ? duplicate() : noPermission();
    }

    onViewEditPage(item) {
        const { selectedPageDirty } = this.props;
        const viewPage = () => PageActions.viewPage(item.id, item.url);

        const left = () => {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                viewPage);
        };

        const right = () => viewPage();
        const proceed = () => selectedPageDirty ? left() : right();

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

    onSearchMoreFlyoutClick() {
        this.setState({ toggleSearchMoreFlyout: !this.state.toggleSearchMoreFlyout }, () => {
            const { toggleSearchMoreFlyout } = this.state;
            !toggleSearchMoreFlyout ? this.setState({ DropdownCalendarIsActive: null }) : null;
        });
    }

    toggleDropdownCalendar(bool) {
        typeof (bool) == "boolean" ? this.setState({ DropdownCalendarIsActive: bool }) : this.setState({ DropdownCalendarIsActive: !this.state.DropdownCalendarIsActive });
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
            const fullStartDate = `${startDate.getDate()}/${startDate.getMonth() + 1}/${startDate.getFullYear()}`;
            const fullEndDate = `${endDate.getDate()}/${endDate.getMonth() + 1}/${endDate.getFullYear()}`;
            const left = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate} - ${fullEndDate} ` });
            const right = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate}` });

            fullStartDate != fullEndDate ? left() : right();
        }

        this.setState({ filters, DropdownCalendarIsActive: null, toggleSearchMoreFlyout: false });
    }
    getDateLabel() {
        let filterByDateText = utils.isPlatform() ? "FilterByModifiedDateText" : "FilterByPublishDateText";
        const { startDate, endDate, startAndEndDateDirty } = this.state;
        let label = Localization.get(filterByDateText);
        if (startAndEndDateDirty) {
            const fullStartDate = `${startDate.getDate()}/${startDate.getMonth() + 1}/${startDate.getFullYear()}`;
            const fullEndDate = `${endDate.getDate()}/${endDate.getMonth() + 1}/${endDate.getFullYear()}`;
            label = fullStartDate !== fullEndDate ? `${fullStartDate} - ${fullEndDate}` : `${fullStartDate}`;
        }
        return label;
    }
    saveSearchFilters(searchFields) {
        return new Promise((resolve) => this.setState({ searchFields }, () => resolve()));
    }

    onSearch() {
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
        this.setState({ DropdownCalendarIsActive: null, toggleSearchMoreFlyout: false });
    }

    doSearch() {
        const { selectedPage } = this.props;
        if (selectedPage) {
            this.onCancelPage();
        }

        let { filtersUpdated, inSearch, searchTerm, filterByPageType, filterByPublishStatus, filterByWorkflow, startDate, endDate, startAndEndDateDirty, tags } = this.state;
        if (filtersUpdated || !inSearch) {
            const fullStartDate = `${startDate.getDate() < 10 ? `0` + startDate.getDate() : startDate.getDate()}/${((startDate.getMonth() + 1) < 10 ? `0` + (startDate.getMonth() + 1) : (startDate.getMonth() + 1))}/${startDate.getFullYear()} 00:00:00`;
            const fullEndDate = `${endDate.getDate() < 10 ? `0` + endDate.getDate() : endDate.getDate()}/${((endDate.getMonth() + 1) < 10 ? `0` + (endDate.getMonth() + 1) : (endDate.getMonth() + 1))}/${endDate.getFullYear()} 23:59:59`;
            const searchDateRange = startAndEndDateDirty ? { publishDateStart: fullStartDate, publishDateEnd: fullEndDate } : {};

            if (tags) {
                tags = tags[0] == "," ? tags.replace(",", "") : tags;
                tags = tags[tags.length - 1] == "," ? tags.split(",").filter(t => !!t).join() : tags;
            }


            let search = { tags: tags, searchKey: searchTerm, pageType: filterByPageType, publishStatus: filterByPublishStatus, workflowId: filterByWorkflow };
            search = Object.assign({}, search, searchDateRange);
            for (let prop in search) {
                if (!search[prop]) {
                    delete search[prop];
                }
            }

            this.generateFilters();
            this.saveSearchFilters(search).then(() => this.props.searchAndFilterPageList(search));
            this.setState({ inSearch: true, filtersUpdated: false });
        }
    }
    clearSearch(callback) {
        let date = new Date();
        this.setState({
            toggleSearchMoreFlyout: false,
            DropdownCalendarIsActive: null,
            filtersUpdated: false,
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
                this._traverse((item, list, update) => {
                    if (item.id === tabId) {
                        page = item;
                        return;
                    }
                });
                //page && page.name && page.id && 
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
    onBreadcrumbSelect(name) {
    }

    isOnInsertMode(){
        return false;
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
            const { props, state } = this;
            const { isContentLocalizationEnabled } = props;
            return (
                <PageSettings
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
                <PageSettings selectedPage={props.selectedPage}
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
    distinct(list) {
        let distinctList = [];
        list.map((item) => {
            if (item.trim() !== "" && distinctList.indexOf(item.trim().toLowerCase()) === -1)
                distinctList.push(item.trim().toLowerCase());
        });
        return distinctList;
    }
    getFilterByPageTypeOptions() {
        return [
            { value: "", label: Localization.get("lblAll") },
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

    /* eslint-disable react/no-danger */
    render_more_flyout() {
        const generateTags = (e) => {
            this.setState({ tags: e.target.value, filtersUpdated: true });
        };
        const filterTags = () => {
            let { tags } = this.state;
            this.setState({ tags: this.distinct(tags.split(",")).join(",") });
        };
        const onApplyChangesDropdownDayPicker = () => {
            const { startAndEndDateDirty, startDate, endDate } = this.state;
            const fullStartDate = startDate.getDate() + startDate.getMonth() + startDate.getFullYear();
            const fullEndDate = endDate.getDate() + endDate.getMonth() + endDate.getFullYear();
            const condition = !startAndEndDateDirty && fullStartDate === fullEndDate;
            condition ? this.setState({ startAndEndDateDirty: true, DropdownCalendarIsActive: null }) : this.setState({ DropdownCalendarIsActive: null });
        };
        return (
            <div className="search-more-flyout">
                <GridCell columnSize={70} style={{ padding: "5px 5px 5px 10px" }}>
                    <h1>{Localization.get("lblGeneralFilters").toUpperCase()}</h1>
                </GridCell>
                <GridCell columnSize={30} style={{ paddingLeft: "10px" }}>
                    <h1>{Localization.get("lblTagFilters").toUpperCase()}</h1>
                </GridCell>
                <GridCell columnSize={70} style={{ padding: "5px" }}>
                    <GridCell columnSize={100} >
                        <GridCell columnSize={50} style={{ padding: "5px" }}>
                            <Dropdown
                                className="more-dropdown"
                                options={this.getFilterByPageTypeOptions()}
                                label={this.state.filterByPageType ? this.getFilterByPageTypeOptions().find(x => x.value === this.state.filterByPageType.toLowerCase()).label : Localization.get("FilterbyPageTypeText")}
                                value={this.state.filterByPageType !== "" && this.state.filterByPageType}
                                onSelect={(data) => { this.setState({ filterByPageType: data.value, filtersUpdated: true }); }}
                                withBorder={true}
                            />
                        </GridCell>
                        <GridCell columnSize={50} style={{ padding: "5px 5px 5px 15px" }}>
                            <Dropdown
                                className="more-dropdown"
                                options={this.getFilterByPageStatusOptions()}
                                label={this.state.filterByPublishStatus ? this.getFilterByPageStatusOptions().find(x => x.value === this.state.filterByPublishStatus.toLowerCase()).label : Localization.get("FilterbyPublishStatusText")}
                                value={this.state.filterByPublishStatus !== "" && this.state.filterByPublishStatus}
                                onSelect={(data) => this.setState({ filterByPublishStatus: data.value, filtersUpdated: true })}
                                withBorder={true} />
                        </GridCell>
                    </GridCell>
                    <GridCell columnSize={100}>
                        <GridCell columnSize={50} style={{ padding: "5px" }}>
                            <DropdownDayPicker
                                onDayClick={this.onDayClick.bind(this)}
                                dropdownIsActive={this.state.DropdownCalendarIsActive}
                                applyChanges={() => onApplyChangesDropdownDayPicker()}
                                startDate={this.state.startDate}
                                endDate={this.state.endDate}
                                toggleDropdownCalendar={this.toggleDropdownCalendar.bind(this)}
                                CalendarIcon={CalendarIcon}
                                label={this.getDateLabel()}
                            />
                        </GridCell>
                        {!utils.isPlatform() &&
                            <GridCell columnSize={50} style={{ padding: "5px 5px 5px 15px" }}>
                                <Dropdown
                                    className="more-dropdown"
                                    options={this.getFilterByWorkflowOptions()}
                                    label={this.state.filterByWorkflow ? this.getFilterByWorkflowOptions().find(x => x.value === this.state.filterByWorkflow).label : Localization.get("FilterbyWorkflowText")}
                                    value={this.state.filterByWorkflow !== "" && this.state.filterByWorkflow}
                                    onSelect={(data) => this.setState({ filterByWorkflow: data.value, filterByWorkflowName: data.label, filtersUpdated: true })}
                                    withBorder={true} />
                            </GridCell>
                        }
                    </GridCell>
                </GridCell>
                <GridCell columnSize={30} style={{ paddingLeft: "10px", paddingTop: "10px" }}>
                    <textarea placeholder={Localization.get("TagsInstructions")} value={this.state.tags} onChange={(e) => generateTags(e)} onBlur={() => filterTags()}></textarea>
                </GridCell>
                <GridCell columnSize={100} style={{ textAlign: "right" }}>
                    <Button style={{ marginRight: "5px" }} onClick={() => this.setState({ DropdownCalendarIsActive: null, toggleSearchMoreFlyout: false })}>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={() => this.onSearch()}>{Localization.get("Save")}</Button>
                </GridCell>
            </div>);
    }

    render_searchResults() {
        const { pageInContextComponents, searchList } = this.props;
        const render_card = (item) => {
            const onNameClick = (item) => {
                this.clearSearch(() => {
                    if (item.canManagePage) {
                        this.onLoadPage(item.id, (data) => { this.buildTree(item.id); });
                    }
                    else {
                        this.noPermissionSelectionPageId = item.id;
                        this.buildBreadCrumbPath(item.id);
                        this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
                    }
                });
            };

            const publishedDate = new Date(item.publishDate.split(" ")[0]);

            const addToTags = (newTag) => {
                const condition = this.state.tags.indexOf(newTag) === -1;
                const update = () => {
                    let tags = this.state.tags;
                    tags = tags.length > 0 ? `${tags},${newTag}` : `${newTag}`;
                    tags = this.distinct(tags.split(",")).join(",");
                    this.setState({ tags, filtersUpdated: true }, () => this.onSearch());
                };

                condition ? update() : null;
            };
            const getTabPath = (path) => {
                path = path.startsWith("/") ? path.substring(1) : path;
                return path.split("/").join(" / ");
            };


            let visibleMenus = [];
            item.canViewPage && visibleMenus.push(<li onClick={() => this.onViewPage(item)}><div title={Localization.get("View")} dangerouslySetInnerHTML={{ __html: EyeIcon }} /></li>);
            item.canAddContentToPage && visibleMenus.push(<li onClick={() => this.onViewEditPage(item)}><div title={Localization.get("Edit")} dangerouslySetInnerHTML={{ __html: TreeEdit }} /></li>);
            if (pageInContextComponents && securityService.isSuperUser() && !utils.isPlatform()) {
                let additionalMenus = cloneDeep(pageInContextComponents || []);
                additionalMenus && additionalMenus.map(additionalMenu => {
                    visibleMenus.push(<li onClick={() => (additionalMenu.OnClickAction && typeof additionalMenu.OnClickAction === "function")
                        && this.CallCustomAction(additionalMenu.OnClickAction)}><div title={additionalMenu.title} dangerouslySetInnerHTML={{ __html: additionalMenu.icon }} /></li>);
                });
            }
            return (
                <GridCell columnSize={100}>
                    <div className="search-item-card">
                        {!utils.isPlatform() &&
                            <div className="search-item-thumbnail">
                                <img src={item.thumbnail} />
                            </div>}
                        <div className={`search-item-details${utils.isPlatform() ? " full" : ""}`}>
                            <div className="search-item-details-left">
                                <h1 onClick={() => onNameClick(item)}><OverflowText text={item.name} /></h1>
                                <h2><OverflowText text={getTabPath(item.tabpath)} /></h2>
                            </div>
                            <div className="search-item-details-right">
                                <ul>
                                    {visibleMenus}
                                </ul>
                            </div>
                            <div className="search-item-details-list">
                                <ul>
                                    <li>
                                        <p>{Localization.get("PageType")}:</p>
                                        <p title={this.getPageTypeLabel(item.pageType)} onClick={() => { this.state.filterByPageType !== item.pageType && this.setState({ filterByPageType: item.pageType, filtersUpdated: true }, () => this.onSearch()); }} >{this.getPageTypeLabel(item.pageType)}</p>
                                    </li>
                                    <li>
                                        <p>{Localization.get("lblPublishStatus")}:</p>
                                        <p title={this.getPublishStatusLabel(item.publishStatus)} onClick={() => { this.state.filterByPublishStatus !== item.publishStatus && this.setState({ filterByPublishStatus: item.publishStatus, filtersUpdated: true }, () => this.onSearch()); }} >{this.getPublishStatusLabel(item.publishStatus)}</p>
                                    </li>
                                    <li>
                                        <p >{Localization.get(utils.isPlatform() ? "lblModifiedDate" : "lblPublishDate")}:</p>
                                        <p title={item.publishDate} onClick={() => { (this.state.startDate.toString() !== new Date(item.publishDate.split(" ")[0]).toString() || this.state.startDate.toString() !== this.state.endDate.toString()) && this.setState({ startDate: publishedDate, endDate: publishedDate, startAndEndDateDirty: true, filtersUpdated: true }, () => this.onSearch()); }}>{item.publishDate.split(" ")[0]}</p>
                                    </li>
                                </ul>
                            </div>
                            <div className="search-item-details-list">
                                <ul>
                                    {!utils.isPlatform() && <li>
                                        <p >{Localization.get("WorkflowTitle")}:</p>
                                        <p title={item.workflowName} onClick={() => { this.state.filterByWorkflow !== item.workflowId && this.setState({ filterByWorkflow: item.workflowId, filterByWorkflowName: item.workflowName, filtersUpdated: true }, () => this.onSearch()); }}>{item.workflowName}</p>
                                    </li>
                                    }
                                    <li style={{ width: !utils.isPlatform() ? "64%" : "99%" }}>
                                        <p>{Localization.get("Tags")}:</p>
                                        <p title={item.tags.join(",").trim(",")}>{
                                            item.tags.map((tag, count) => {
                                                return (
                                                    <span>
                                                        <span style={{ marginLeft: "5px" }} onClick={() => addToTags(tag)}>
                                                            {tag}
                                                        </span>
                                                        {count < (item.tags.length - 1) && <span style={{ color: "#000" }}>
                                                            ,
                                                        </span>}
                                                    </span>
                                                );
                                            })}
                                        </p>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </GridCell >
            );
        };

        return (
            <GridCell columnSize={100} className="fade-in">
                <GridCell columnSize={100} style={{ padding: "20px" }}>
                    <GridCell columnSize={80} style={{ padding: "0px" }}>
                        <div className="tags-container">
                            {this.state.filters ? this.render_filters() : null}
                        </div>
                    </GridCell>
                    <GridCell columnSize={20} style={{ textAlign: "right", padding: "10px", fontWeight: "bold", animation: "fadeIn .15s ease-in forwards" }}>
                        <p>{searchList.length === 0 ? Localization.get("NoPageFound").toUpperCase() : (`${searchList.length} ` + Localization.get(searchList.length > 1 ? "lblPagesFound" : "lblPageFound").toUpperCase())}</p>
                    </GridCell>
                    <GridCell columnSize={100}>
                        {searchList.map((item) => {
                            return render_card(item);
                        })}
                    </GridCell>
                </GridCell>
            </GridCell>
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

        switch (true){
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

    render_filters() {
        const { filters } = this.state;
        return filters
            .filter(filter => !!filter)
            .map((filter) => {

                const deleteFilter = (prop) => {
                    const left = () => {
                        const update = {};
                        update[prop] = null;
                        if (prop === "startAndEndDateDirty") {
                            this.setState({ startDate: new Date(), endDate: new Date() });
                        }
                        this.setState({ filtersUpdated: true }, () => {
                            this.setState(update, () => this.onSearch());
                        });
                    };
                    const right = () => {
                        let { filters, tags } = this.state;
                        tags = this.distinct(tags.split(",")).join(",");
                        filters = filters.filter(f => f.ref != prop);
                        const findTag = prop.replace("tag-", "");
                        let tagList = tags.split(",");
                        tags = "";
                        tagList.map((tag) => {
                            if (tag !== findTag)
                                tags += tag + ",";
                        });
                        tags = tags !== "" ? tags.substring(0, tags.length - 1) : "";
                        this.setState({ filters, tags, filtersUpdated: true }, () => this.onSearch());

                    };
                    const condition = prop.indexOf('tag') === -1;
                    condition ? left() : right();
                };

                return (
                    <div className="filter-by-tags">
                        <OverflowText text={filter.tag} maxWidth={300} />
                        <div className="xIcon"
                            dangerouslySetInnerHTML={{ __html: XIcon }}
                            onClick={(e) => { deleteFilter(filter.ref); }}>

                        </div>
                    </div>
                );
            });
    }

    render() {

        const { props } = this;
        const { selectedPage } = props;
        const { inSearch, headerDropdownSelection, toggleSearchMoreFlyout, searchTerm } = this.state;

        const isListPagesAllowed = securityService.canSeePagesList();
       
         /* eslint-disable react/no-danger */
        return (

            <div className="pages-app personaBar-mainContainer">
                { isListPagesAllowed &&
                    <PersonaBarPage fullWidth={true} isOpen={true}>
                        <PersonaBarPageHeader title={Localization.get(inSearch ? "PagesSearchHeader" : "Pages")}>
                            {securityService.isSuperUser() &&
                                <div> 
                                    <Button type="primary" disabled={ this.onEditMode() } size="large" onClick={this.onAddPage.bind(this)}>{Localization.get("AddPage")}</Button>
                                    <Button type="secondary" disabled={ this.onEditMode() } size="large" onClick={this.onAddMultiplePage.bind(this)}>{Localization.get("AddMultiplePages")}</Button>
                                </div>
                            }
                            { 
                                selectedPage && this.getSettingsButtons()
                            }                            
                            {!inSearch && <BreadCrumbs items={this.props.selectedPagePath || []} onSelectedItem={this.onSelection.bind(this)} />}
                        </PersonaBarPageHeader>
                        {toggleSearchMoreFlyout ? this.render_more_flyout() : null}
                        <GridCell columnSize={100} style={{ padding: "30px 30px 16px 30px" }}>
                            <div className="search-container">
                                {inSearch ?
                                    <div className="dnn-back-to-link" onClick={() => this.clearSearch()}>
                                        <div className="dnn-back-to-arrow" dangerouslySetInnerHTML={{ __html: ArrowBack }} /> <span>{Localization.get("BackToPages").toUpperCase()}</span>
                                    </div> : null
                                }

                                <div className="search-box">
                                    <div className="search-input">
                                        <input
                                            type="text"
                                            value={searchTerm}
                                            onFocus={this.onSearchFocus.bind(this)}
                                            onChange={this.onSearchFieldChange.bind(this)}
                                            onKeyPress={(e) => { e.key === "Enter" ? this.onSearch() : null; }}
                                            placeholder="Search" />
                                    </div>
                                    {searchTerm ?
                                        <div
                                            className="btn clear-search"
                                            style={{ fill: "#444" }}
                                            dangerouslySetInnerHTML={{ __html: XIcon }}
                                            onClick={() => this.setState({ searchTerm: "", filtersUpdated: true }, () => this.onSearch())}
                                        />

                                        : <div className="btn clear-search" />}
                                    <div
                                        className="btn search-btn"
                                        dangerouslySetInnerHTML={{ __html: PagesSearchIcon }}
                                        onClick={this.onSearch.bind(this)}
                                    >
                                    </div>
                                    <div
                                        className="btn search-btn"
                                        dangerouslySetInnerHTML={{ __html: PagesVerticalMore }}
                                        onClick={() => { this.onSearchMoreFlyoutClick(); }}
                                    />
                                </div>
                            </div>
                        </GridCell>
                        <GridCell columnSize={100} style={{ padding: "0px 30px 30px 30px" }} >
                            <GridCell columnSize={1096} type={"px"} className="page-container">
                                <div className={((selectedPage && selectedPage.tabId === 0) || this.onEditMode()) ? "tree-container disabled" : "tree-container"}>
                                    <PersonaBarPageTreeviewInteractor
                                        clearSelectedPage={this.props.clearSelectedPage}
                                        Localization={Localization}
                                        pageList={this.props.pageList}
                                        getChildPageList={this.props.getChildPageList}
                                        getPage={this.props.getPage.bind(this)}
                                        _traverse={this._traverse.bind(this)}
                                        showCancelDialog={this.showCancelWithoutSavingDialogInEditMode.bind(this)}
                                        selectedPageDirty={this.props.selectedPageDirty}
                                        activePage={this.props.selectedPage}
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
    dispatch: PropTypes.func.isRequired,
    pageList: PropTypes.array.isRequired,
    searchList: PropTypes.array.isRequired,
    searchPageList: PropTypes.func.isRequired,
    searchAndFilterPageList: PropTypes.func.isRequired,
    getChildPageList: PropTypes.func.isRequired,
    getWorkflowsList: PropTypes.func.isRequired,
    selectedView: PropTypes.number,
    selectedPage: PropTypes.object,
    selectedPageErrors: PropTypes.object,
    selectedPageDirty: PropTypes.boolean,
    selectedTemplateDirty: PropTypes.boolean,
    bulkPage: PropTypes.object,
    dirtyBulkPage : PropTypes.boolean, 
    editingSettingModuleId: PropTypes.number,
    onCancelPage: PropTypes.func.isRequired,
    onCreatePage: PropTypes.func.isRequired,
    onUpdatePage: PropTypes.func.isRequired,
    onDeletePage: PropTypes.func.isRequired,
    getPageList: PropTypes.func.isRequired,
    getPage: PropTypes.func.isRequired,
    updatePageListStore: PropTypes.func.isRequire,
    getNewPage: PropTypes.func.isRequired,
    onLoadPage: PropTypes.func.isRequired,
    onCancelAddMultiplePages: PropTypes.func.isRequired,
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
    isContentLocalizationEnabled: PropTypes.object.isRequired,
    getContentLocalizationEnabled: PropTypes.func.isRequired,
    selectPage: PropTypes.func.isRequired,
    selectedPagePath: PropTypes.array.isRequired,
    changeSelectedPagePath: PropTypes.func.isRequired,
    onGetCachedPageCount: PropTypes.array.isRequired,
    onClearCache: PropTypes.func.isRequired,
    clearSelectedPage: PropTypes.func.isRequired,
    onModuleCopyChange: PropTypes.func,
    workflowList: PropTypes.array.isRequired,
    customPageSettingsComponents: PropTypes.array,
    getPageHierarchy: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        pageList: state.pageList.pageList,
        searchList: state.searchList.searchList,
        selectedView: state.visiblePanel.selectedPage,
        selectedCustomPageSettings : state.visiblePageSettings.panelId,
        selectedPage: state.pages.selectedPage,
        selectedPageErrors: state.pages.errors,
        selectedPageDirty: state.pages.dirtyPage,
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
        customPageSettingsComponents : state.extensions.pageSettingsComponent
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators({
        getNewPage: PageActions.getNewPage,
        getPageList: PageActions.getPageList,
        searchPageList: PageActions.searchPageList,
        searchAndFilterPageList: PageActions.searchAndFilterPageList,
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
        getPageHierarchy: PageActions.getPageHierarchy

    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(App);
