import ActionTypes from "../constants/actionTypes/pageActionTypes";
import PageListActionTypes from "../constants/actionTypes/pageListActionTypes";
import SearchListActionTypes from "../constants/actionTypes/searchListActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";
import debounce from "lodash/debounce";
import cloneDeep from "lodash/cloneDeep";
import securityService from "../services/securityService";
import permissionTypes from "../services/permissionTypes";
import Promise from "promise";


function updateUrlPreview(value, dispatch) {
    PagesService.getPageUrlPreview(value).then(response => {
        dispatch({
            type: ActionTypes.CHANGE_FIELD_VALUE,
            urlPreviewChange: true,
            field: "url",
            value: response.Url
        });
    }).catch(() => {
        dispatch({
            type: ActionTypes.ERROR_LOADING_PAGE
        });
    });
}

const debouncedUpdateUrlPreview = debounce(updateUrlPreview, 500);

const loadPage = function (dispatch, pageId) {
    return new Promise((resolve)=>{
            if (!securityService.userHasPermission(permissionTypes.MANAGE_PAGE)) {
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: {
                    page: {
                        tabId: utils.getCurrentPageId(),
                        name: utils.getCurrentPageName()
                    }
                }
            });
            resolve();
            return;
        }

        PagesService.getPage(pageId).then(response => {
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: {
                    page: response
                }
            });
            resolve(response);
        }).catch((error) => {
            dispatch({
                type: ActionTypes.ERROR_LOADING_PAGE,
                data: { error }
            });
            resolve();
        });

    });
};

const pageActions = {
    getPageList(id) {
        return (dispatch) => PagesService.getPageList(id).then(pageList => {
            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        });
    },

    searchPageList(searchKey) {
        return (dispatch) => PagesService.searchPageList(searchKey).then((searchList) => {
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: { searchList }
            });
        });
    },

    searchAndFilterPageList(params) {
        return (dispatch) => PagesService.searchAndFilterPageList(params).then((searchList)=>{
            searchList= searchList.Results;
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: {searchList}
            });
        });
    },

    getWorkflowsList(){
        return (dispatch) => PagesService.getWorkflowsList();
    },

    getPage(id) {
        return (dispatch) => PagesService.getPage(id);
    },

    getCurrentSelectedPage(){
        return (dispatch) => dispatch({
            type: ActionTypes.GET_CURRENT_SELECTED_PAGE,
            data:{}
        });
    },
    getChildPageList(id) {
        return () => PagesService.getChildPageList(id);
    },

    updatePageListStore(pageList) {
        return (dispatch) => {
            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        };
    },

    selectPageSettingTab(selectedPageSettingTab) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECT_PAGE_SETTING_TAB,
                selectedPageSettingTab
            });
        };
    },

    loadPage(pageId) {
        return (dispatch) => {
            return loadPage(dispatch, pageId);
        };
    },

    viewPage(id, url) {
        PagesService.openPageInEditMode(id, url);
        return (dispatch) => {

        };
    },

    duplicatePage() {
        return (dispatch, getState) => {
            const { pages } = getState();
            const duplicatedPage = cloneDeep(pages.selectedPage);

            duplicatedPage.templateTabId = duplicatedPage.tabId;
            duplicatedPage.tabId = 0;
            duplicatedPage.name = "";
            duplicatedPage.url = "";

            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: {
                    page: duplicatedPage
                }
            });
        };
    },


    getNewPage(parentPage) {
        return (dispatch) => PagesService.getNewPage(parentPage).then((page) => {
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: { page }
            });
        });
    },

    cancelPage() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCEL_PAGE,
                data: {}
            });
        };
    },

    deletePage(page) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DELETE_PAGE
            });

            PagesService.deletePage(page).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                dispatch({
                    type: ActionTypes.DELETED_PAGE
                });
                if (page.tabId !== 0 && page.tabId === utils.getCurrentPageId()) {
                    window.top.location.href = utils.getDefaultPageUrl();
                }

            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE,
                    data: { error }
                });
            });
        };
    },
    createPage() {

        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SAVE_PAGE
            });
            const { pages } = getState();
            const selectedPage = pages.selectedPage;

            PagesService.savePage(selectedPage, pages.urlChanged).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                if (selectedPage.tabId > 0) {
                    utils.notify(Localization.get("PageUpdatedMessage"));
                }
                PagesService.openPageInEditMode(response.Page.id, response.Page.url);

            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: { error }
                });
            });
        };
    },

    updatePage(page, callback) {
        return (dispatch, getState) => {

            const { pages } = getState();

            PagesService.savePage(page, pages.urlChanged).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                if (page.tabId > 0) {
                    utils.notify(Localization.get("PageUpdatedMessage"));
                }

                dispatch({
                    type: ActionTypes.SAVE_PAGE,
                    data: null
                });

                callback(page);

            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: { error }
                });
            });
        };
    },


    changePageField(key, value) {
        return (dispatch, getState) => {
            const { pages } = getState();

            dispatch({
                type: ActionTypes.CHANGE_FIELD_VALUE,
                field: key,
                value
            });

            if (key === "name" &&
                pages.selectedPage.tabId === 0 &&
                !pages.urlChanged &&
                pages.selectedPage.pageType === "normal") {
                debouncedUpdateUrlPreview(value, dispatch);
            }
        };
    },

    changePageType(value) {
        return {
            type: ActionTypes.CHANGE_FIELD_VALUE,
            field: "pageType",
            value
        };
    },

    changePermissions(permissions) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_PERMISSIONS,
                permissions
            });
        };
    },

    fetchCacheProviderList() {
        return (dispatch, getState) => {
            if (!getState().pages.cacheProviderList) {
                dispatch({
                    type: ActionTypes.FETCH_CACHE_PROVIDER_LIST
                });

                PagesService.getCacheProviderList().then(cacheProviderList => {
                    dispatch({
                        type: ActionTypes.FETCHED_CACHE_PROVIDER_LIST,
                        data: { cacheProviderList }
                    });
                }).catch((error) => {
                    dispatch({
                        type: ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST,
                        data: { error }
                    });
                });
            }
        };
    },

    deletePageModule(module) {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.DELETING_PAGE_MODULE
            });

            const pageId = getState().pages.selectedPage.tabId;
            const moduleToDelete = {
                moduleId: module.id,
                pageId
            };
            PagesService.deletePageModule(moduleToDelete).then(() => {
                utils.notify(Localization.get("DeletePageModuleSuccess").replace("[MODULETITLE]", module.title));
                dispatch({
                    type: ActionTypes.DELETED_PAGE_MODULE,
                    data: { module }
                });
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE_MODULE,
                    data: { error }
                });
            });
        };
    },

    updatePageModuleCopy(id, key, event) {
        return {
            type: ActionTypes.UPDATED_PAGE_MODULE_COPY,
            data: { id, key, event }
        };
    },

    editingPageModule(module) {
        return {
            type: ActionTypes.EDITING_PAGE_MODULE,
            data: { module }
        };
    },

    cancelEditingPageModule() {
        return {
            type: ActionTypes.CANCEL_EDITING_PAGE_MODULE,
            data: {}
        };
    },

    copyAppearanceToDescendantPages() {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.COPYING_APPEARANCE_TO_DESCENDANT_PAGES
            });

            const state = getState();
            const page = state.pages.selectedPage;
            const { defaultPortalLayout, defaultPortalContainer } = state.theme;
            const theme = {
                skinSrc: page.skinSrc || defaultPortalLayout,
                containerSrc: page.containerSrc || defaultPortalContainer
            };

            if (!theme.skinSrc || !theme.containerSrc) {
                utils.notifyError(Localization.get("PleaseSelectLayoutContainer"));
                return;
            }

            PagesService.copyAppearanceToDescendantPages(page.tabId, theme).then(() => {
                utils.notify(Localization.get("CopyAppearanceToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: {}
                });
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_COPYING_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    copyPermissionsToDescendantPages() {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.COPYING_PERMISSIONS_TO_DESCENDANT_PAGES
            });

            const page = getState().pages.selectedPage;
            PagesService.copyPermissionsToDescendantPages(page.tabId).then(() => {
                utils.notify(Localization.get("CopyPermissionsToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: {}
                });
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_COPYING_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    getCachedPageCount(cacheProvider) {
        return (dispatch, getState) => {
            const page = getState().pages.selectedPage;
            PagesService.getCachedPageCount(cacheProvider, page.tabId).then(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CACHED_PAGE_COUNT,
                    data: {
                        cachedPageCount: data.Count
                    }
                });
            });
        };
    },

    clearCache(cacheProvider) {
        return (dispatch, getState) => {
            const page = getState().pages.selectedPage;
            PagesService.clearCache(cacheProvider, page.tabId).then(() => {
                dispatch({
                    type: ActionTypes.CLEARED_CACHED_PAGE,
                    data: {
                        cachedPageCount: 0
                    }
                });
            });
        };
    },

    clearSelectedPage() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_SELECTED_PAGE,
                data: {}
            });
        };
    },

    movePage({ Action, PageId, ParentId, RelatedPageId }) {
        return PagesService.movePage({ Action, PageId, ParentId, RelatedPageId });
    }
};

export default pageActions;