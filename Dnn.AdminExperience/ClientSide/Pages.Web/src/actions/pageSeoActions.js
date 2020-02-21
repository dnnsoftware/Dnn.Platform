import PageSeoService from "../services/pageSeoService";
import { pageSeoTypes as ActionTypes } from "../constants/actionTypes";
import pageActionTypes from "../constants/actionTypes/pageActionTypes";
import utils from "../utils";
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
    openEditForm(url) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_OPEN_EDIT_FORM,
                payload: {
                    url
                }
            });
        };
    },
    closeEditForm() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CLOSE_EDIT_FORM
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
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_ADD_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.add(url, tabId, primaryAliasId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_ADDING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    utils.notifyError(response.ErrorMessage);

                    // DNN-30998: Adding SuggestedUrl returned by the server in case of validation errors
                    if(response.SuggestedUrlPath)
                    {
                        dispatch({
                            type: ActionTypes.SEO_CHANGE_URL,
                            payload: {
                                key: "path",
                                value: response.SuggestedUrlPath
                            }
                        });
                    }
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_ADDED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.ADD_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SEO_ADDING_URL,
                    data: { error }
                });
            });
        };
    },
    saveUrl(url, primaryAliasId) {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_SAVE_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.save(url, tabId, primaryAliasId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_SAVING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    utils.notifyError(response.ErrorMessage);
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_SAVED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.REPLACE_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SEO_SAVING_URL,
                    data: { error: error.ErrorMessage }
                });
            });
        };
    },
    deleteUrl(url) {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_DELETE_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.delete(url, tabId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_DELETING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_DELETED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.DELETE_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });                
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SEO_DELETING_URL,
                    data: { error: error.ErrorMessage }
                });
            });
        };
    }
};

export default pageSeoActions;
