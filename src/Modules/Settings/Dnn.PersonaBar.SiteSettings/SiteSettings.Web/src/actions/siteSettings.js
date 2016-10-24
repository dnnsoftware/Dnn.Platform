import {siteSettings as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const siteSettingsActions = {
    getPortalSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getPortalSettings(portalId, data => {
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
    },
    getDefaultPagesSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getDefaultPagesSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_DEFAULT_PAGES_SETTINGS,
                    data: {
                        settings: data.Settings,
                        defaultPagesSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateDefaultPagesSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateDefaultPagesSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_DEFAULT_PAGES_SETTINGS,
                    data: {
                        defaultPagesSettingsClientModified: false
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
    defaultPagesSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_DEFAULT_PAGES_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    defaultPagesSettingsClientModified: true
                }
            });
        };
    },
    getMessagingSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getMessagingSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_MESSAGING_SETTINGS,
                    data: {
                        settings: data.Settings,
                        messagingSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateMessagingSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateMessagingSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_MESSAGING_SETTINGS,
                    data: {
                        messagingSettingsClientModified: false
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
    messagingSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_MESSAGING_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    messagingSettingsClientModified: true
                }
            });
        };
    },
    getProfileSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getProfileSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_SETTINGS,
                    data: {
                        settings: data.Settings,
                        userVisibilityOptions: data.UserVisibilityOptions,
                        profileSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateProfileSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateProfileSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_SETTINGS,
                    data: {
                        profileSettingsClientModified: false
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
    profileSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    profileSettingsClientModified: true
                }
            });
        };
    }
};

export default siteSettingsActions;
