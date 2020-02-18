import { siteInfo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const siteInfoActions = {
    updatePortalId(portalId) {
        return {
            type: ActionTypes.CHANGED_PORTAL_ID,
            data: {
                portalId
            }
        };
    },
    getPortals(callback) {
        return (dispatch) => {
            ApplicationService.getPortals(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    data: {
                        portals: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPortalSettings(portalId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getPortalSettings(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_SETTINGS,
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
                    type: ActionTypes.UPDATED_SITESETTINGS_PORTAL_SETTINGS,
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
                type: ActionTypes.SITESETTINGS_PORTAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    clientModified: true
                }
            });
        };
    }
};

export default siteInfoActions;
