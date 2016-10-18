import {siteInfo as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const siteInfoActions = {
    getPortalSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getPortalSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITEINFO_PORTAL_SETTINGS,
                    data: {
                        settings: data.Settings,
                        timeZones: data.TimeZones,
                        iconSets: data.IconSets,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePortalSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updatePortalSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITEINFO_PORTAL_SETTINGS,
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
    portalSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITEINFO_PORTAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    clientModified: true
                }
            });
        };
    }
};

export default siteInfoActions;
