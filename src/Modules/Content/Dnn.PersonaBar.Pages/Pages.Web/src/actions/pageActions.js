import ActionTypes from "../constants/actionTypes/pageActionTypes";
import PagesService from "../services/pageService";

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
                        page: PagesService.toFrontEndPage(response)
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE
                });
            });     
        };
    },

    addPage() {
        return (dispatch) => {
            PagesService.getNewPage().then(page => {
                dispatch({
                    type: ActionTypes.LOADED_PAGE,
                    data: { page }
                });
            });
        };
    },

    savePage(page) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SAVE_PAGE
            });    

            PagesService.savePage(page).then(response => {
                dispatch({
                    type: ActionTypes.SAVED_PAGE,
                    data: {
                        createdPage: page.tabId === 0 ? response.Page : null 
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE
                });
            });     
        };
    },

    changePageField(key, value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_FIELD_VALUE,
                field: key,
                value
            });  
        };
    },

    changePageType(value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_FIELD_VALUE,
                field: "pageType",
                value
            });  
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
                }).catch(() => {
                    dispatch({
                        type: ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST
                    });
                });                     
            }
        };
    },

    deletePageModule(module) {
        return (dispatch) => {            
            dispatch({
                type: ActionTypes.DELETING_PAGE_MODULE
            });

            PagesService.deletePageModule(module).then(() => {
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
    }
};

export default pageActions;