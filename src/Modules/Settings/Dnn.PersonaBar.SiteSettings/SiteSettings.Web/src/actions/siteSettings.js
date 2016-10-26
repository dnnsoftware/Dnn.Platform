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
    },
    getProfileProperties(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getProfileProperties(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTIES,
                    data: {
                        profileProperties: data.ProfileProperties                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getProfileProperty(propertyId, portalId, callback) {
        return (dispatch) => {
            ApplicationService.getProfileProperty(propertyId, portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profileProperty: data.ProfileProperty,
                        userVisibilityOptions: data.UserVisibilityOptions,
                        dataTypeOptions: data.DataTypeOptions,
                        languageOptions: data.LanguageOptions,
                        profilePropertyClientModified: false                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    profilePropertyClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED,
                data: {
                    profileProperty: parameter,
                    profilePropertyClientModified: true
                }
            });
        };
    },
    propertyLocalizationClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED,
                data: {
                    propertyLocalization: parameter,
                    propertyLocalizationClientModified: true
                }
            });
        };
    },
    cancelProfilePropertyClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED,
                data: {
                    profilePropertyClientModified: false
                }
            });
        };
    },
    cancelPropertyLocalizationClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED,
                data: {
                    propertyLocalizationClientModified: false
                }
            });
        };
    },
    getProfilePropertyLocalization(propertyName, propertyCategory, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getProfilePropertyLocalization(propertyName, propertyCategory, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION,
                    data: {
                        propertyLocalization: data.PropertyLocalization,
                        propertyLocalizationClientModified: false                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateProfileProperty(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateProfileProperty(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profilePropertyClientModified: false
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
    addProfileProperty(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addProfileProperty(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profilePropertyClientModified: false
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
    }
};

export default siteSettingsActions;
