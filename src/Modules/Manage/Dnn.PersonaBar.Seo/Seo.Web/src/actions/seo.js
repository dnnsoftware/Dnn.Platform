import {seo as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const siteInfoActions = {
    getGeneralSettings(callback) {
        return (dispatch) => {
            ApplicationService.getGeneralSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS,
                    data: {
                        settings: data.Settings,
                        replacementCharacterList: data.ReplacementCharacterList,
                        deletedPageHandlingTypes: data.DeletedPageHandlingTypes,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateGeneralSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateGeneralSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_GENERAL_SETTINGS,
                    data: {
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    generalSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    clientModified: true
                }
            });
        };
    }
};

export default siteInfoActions;
