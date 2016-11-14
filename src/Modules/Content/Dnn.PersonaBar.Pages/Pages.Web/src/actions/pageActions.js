import ActionTypes from "../constants/actionTypes/pageActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";
import debounce from "lodash/debounce";

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

const pageActions = {
    loadPage(pageId) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_PAGE
            });    

            PagesService.getPage(pageId).then(response => {
                dispatch({
                    type: ActionTypes.LOADED_PAGE,
                    data: {
                        page: response
                    }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE,
                    data: {error}
                });
            });     
        };
    },

    addPage() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_PAGE
            }); 
            
            PagesService.getNewPage().then(page => {
                dispatch({
                    type: ActionTypes.LOADED_PAGE,
                    data: { page }
                });
            });
        };
    },

    cancelPage() {
        return {
            type: ActionTypes.CANCEL_PAGE,
            data: {}
        };
    },

    savePage(page) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SAVE_PAGE
            });    

            PagesService.savePage(page).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(Localization.get("Error_" + response.Message), 3000);
                    return;
                }

                dispatch({
                    type: ActionTypes.SAVED_PAGE,
                    data: {
                        createdPage: page.tabId === 0 ? response.Page : null 
                    }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: {error}
                });
            });     
        };
    },

    changePageField(key, value) {
        return (dispatch, getState) => {
            const {pages} = getState();
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
        return {
            type: ActionTypes.CHANGE_PERMISSIONS,
            permissions
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
                        data: {error}
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
                    data: {error}
                });
            });      
        };
    },

    toggleEditPageModule(module) {
        return {
            type: ActionTypes.TOGGLE_EDIT_PAGE_MODULE,
            data: {module}
        };
    },

    copyAppearanceToDescendantPages() {
        return (dispatch, getState) => {            
            dispatch({
                type: ActionTypes.COPYING_APPEARANCE_TO_DESCENDANT_PAGES
            });

            const page = getState().pages.selectedPage;
            const theme = {
                skinSrc: page.skinSrc, 
                containerSrc: page.containerSrc
            };
            PagesService.copyAppearanceToDescendantPages(page.tabId, theme).then(() => {
                utils.notify(Localization.get("CopyAppearanceToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: { }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_COPYING_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: {error}
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
                    data: { }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_COPYING_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: {error}
                });
            });      
        };
    }
};

export default pageActions;