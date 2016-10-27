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
                        page: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE
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
                        page: response
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

    changePermissions(permissions) {
        return {
            type: ActionTypes.CHANGE_PERMISSIONS,
            permissions
        };
    }
};

export default pageActions;