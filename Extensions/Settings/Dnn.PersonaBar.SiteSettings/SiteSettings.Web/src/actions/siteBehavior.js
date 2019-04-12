import { siteBehavior as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const siteBehaviorActions = {
    getDefaultPagesSettings(portalId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getDefaultPagesSettings(portalId, cultureCode, data => {
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
                        profileProperties: data
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
    getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, data => {
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
    },
    deleteProfileProperty(propertyId, object, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteProfileProperty(propertyId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profileProperties: object
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
    updateProfilePropertyLocalization(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateProfilePropertyLocalization(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION,
                    data: {
                        propertyLocalizationClientModified: false
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
    getUrlMappingSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getUrlMappingSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS_SETTINGS,
                    data: {
                        urlMappingSettings: data.Settings,
                        portalAliasMappingModes: data.PortalAliasMappingModes,
                        urlMappingSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSiteAliases(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getSiteAliases(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIASES,
                    data: {
                        siteAliases: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    urlMappingSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_URL_MAPPING_SETTINGS_CLIENT_MODIFIED,
                data: {
                    urlMappingSettings: parameter,
                    urlMappingSettingsClientModified: true
                }
            });
        };
    },
    updateUrlMappingSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateUrlMappingSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_URL_MAPPING_SETTINGS,
                    data: {
                        urlMappingSettingsClientModified: false
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
    getSiteAlias(aliasId, callback) {
        return (dispatch) => {
            ApplicationService.getSiteAlias(aliasId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS,
                    data: {
                        aliasDetail: data.PortalAlias
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    cancelSiteAliasClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED,
                data: {
                    siteAliasClientModified: false
                }
            });
        };
    },
    siteAliasClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED,
                data: {
                    aliasDetail: parameter,
                    siteAliasClientModified: true
                }
            });
        };
    },
    addSiteAlias(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addSiteAlias(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliasClientModified: false
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
    updateSiteAlias(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateSiteAlias(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliasClientModified: false
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
    deleteSiteAlias(aliasId, aliases, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteSiteAlias(aliasId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliases: aliases
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
    getPrivacySettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getPrivacySettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PRIVACY_SETTINGS,
                    data: {
                        settings: data.Settings,
                        privacySettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePrivacySettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updatePrivacySettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PRIVACY_SETTINGS,
                    data: {
                        privacySettingsClientModified: false
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
    resetTermsAgreement(payload, callback, failureCallback) {
        ApplicationService.resetTermsAgreement(payload, data => {
            if (callback) {
                callback(data);
            }
        }, data => {
            if (failureCallback) {
                failureCallback(data);
            }
        });
    },
    privacySettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PRIVACY_SETTINGS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    privacySettingsClientModified: true
                }
            });
        };
    },
    getOtherSettings(callback) {
        return (dispatch) => {
            ApplicationService.getOtherSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_OTHER_SETTINGS,
                    data: {
                        settings: data.Settings,
                        otherSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateOtherSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateOtherSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_OTHER_SETTINGS,
                    data: {
                        otherSettingsClientModified: false
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
    otherSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_OTHER_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    otherSettingsClientModified: true
                }
            });
        };
    },
    cancelOtherSettingsClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_OTHER_SETTINGS_CLIENT_MODIFIED,
                data: {
                    otherSettingsClientModified: false
                }
            });
        };
    },
    updateProfilePropertyOrders(payload, object, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateProfilePropertyOrders(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER,
                    data: {
                        profileProperties: object

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
    sortProfileProperty(profileProperties) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER,
                data: { profileProperties }
            });
        };
    },
    getListInfo(listName, portalId, callback) {
        return (dispatch) => {
            ApplicationService.getListInfo(listName, portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LIST_INFO,
                    data: {
                        enableSortOrder: data.EnableSortOrder,
                        entries: data.Entries
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateListEntry(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateListEntry(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LIST_ENTRY,
                    data: {
                        
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
    deleteListEntry(entryId, portalId, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteListEntry(entryId, portalId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_LIST_ENTRY,
                    data: {
                        
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
    updateListEntryOrders(payload, object, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateListEntryOrders(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LIST_ENTRY_ORDER,
                    data: {
                        entries: object

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

export default siteBehaviorActions;
