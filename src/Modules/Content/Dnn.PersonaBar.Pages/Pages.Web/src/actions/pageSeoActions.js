import PageSeoService from "../services/pageSeoService";
import {pageSeoTypes as ActionTypes}  from "../constants/actionTypes";
import pageActionTypes  from "../constants/actionTypes/pageActionTypes";
const pageSeoActions = {
    openNewForm() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_OPEN_NEW_FORM
            });
        };
    },
    closeNewForm() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CLOSE_NEW_FORM
            });
        };
    },
    change(key, value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CHANGE_URL,
                payload: {
                    key: key,
                    value: value
                }
            });
        };        
    },
    addUrl(url, primaryAliasId) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_ADD_URL
            });

            PageSeoService.add(url, primaryAliasId).then((response) => {
                const newUrl = {
                    ...url
                };  
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_ADDING_URL,
                        data: {error: response.ErrorMessage}
                    });
                    return;
                }
                newUrl.Id = response.Id;
                dispatch({
                    type: ActionTypes.SEO_ADDED_URL,
                    payload: {
                        newUrl
                    }
                });
                dispatch({
                    type: pageActionTypes.ADD_CUSTOM_URL,
                    payload: {
                        newUrl
                    }
                });                                
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SEO_ADDING_URL,
                    data: {error}
                });
            }); 
        };
    }
};

export default pageSeoActions;
