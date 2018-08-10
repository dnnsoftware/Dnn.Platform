import { languages as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import LanguageEditorService from "services/languageEditorService";

const languagesActions = {
    getLanguageSettings(portalId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getLanguageSettings(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data: {
                        languageSettings: data.Settings,
                        languages: data.Languages,
                        languageDisplayModes: data.LanguageDisplayModes,
                        languageSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    languageSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_LANGUAGE_SETTINGS_CLIENT_MODIFIED,
                data: {
                    languageSettings: parameter,
                    languageSettingsClientModified: true
                }
            });
        };
    },
    updateLanguageSettings(payload, languageList, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateLanguageSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data: {
                        languageList: languageList,
                        siteDefaultLanguage: payload.SiteDefaultLanguage,
                        languageSettingsClientModified: false
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
    getLanguages(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getLanguages(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGES,
                    data: {
                        languageList: data.Languages
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLanguage(portalId, languageId, callback) {
        return (dispatch) => {
            ApplicationService.getLanguage(portalId, languageId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE,
                    data: {
                        language: data.Language,
                        fallbacks: data.SupportedFallbacks,
                        languageClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAllLanguages(callback) {
        return (dispatch) => {
            ApplicationService.getAllLanguages(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_ALL_LANGUAGES,
                    data: {
                        fullLanguageList: data.FullLanguageList
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    languageClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_LANGUAGE_CLIENT_MODIFIED,
                data: {
                    language: parameter,
                    languageClientModified: true
                }
            });
        };
    },
    cancelLanguageClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_LANGUAGE_CLIENT_MODIFIED,
                data: {
                    ignoreWordsClientModified: false
                }
            });
        };
    },
    addLanguage(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addLanguage(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_LANGUAGE,
                    data: {
                        languageClientModified: false
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
    updateLanguage(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateLanguage(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE,
                    data: {
                        languageClientModified: false
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
    updateLanguageRoles(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateLanguageRoles(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLES,
                    data: {
                        languageClientModified: false
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
    verifyLanguageResourceFiles(callback) {
        return (dispatch) => {
            ApplicationService.verifyLanguageResourceFiles(data => {
                dispatch({
                    type: ActionTypes.VERIFIED_SITESETTINGS_LANGUAGE_RESOURCE_FILES,
                    data: {
                        verificationResults: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getModuleList(type, callback) {
        return (dispatch) => {
            ApplicationService.getModuleList(type, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_PACK_MODULE_LIST,
                    data: {
                        modules: data.Modules
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createLanguagePack(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.createLanguagePack(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_LANGUAGE_PACK,
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
    disableLocalizedContent(portalId, callback) {
        return () => {
            LanguageEditorService.disableLocalizedContent(portalId, callback);
        };
    },
    deleteLanguagePages(payload, callback) {
        return (dispatch) => {
            LanguageEditorService.deleteLanguagePages(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_LANGUAGE_PAGES,
                    data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    publishAllPages(payload, callback, failureCallback) {
        return (dispatch) => {
            LanguageEditorService.publishAllPages(payload, data => {
                dispatch({
                    type: ActionTypes.PUBLISHED_LOCALIZED_PAGES,
                    data
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
    enableLocalizedContent(payload, callback, failureCallback) {
        return (dispatch) => {
            LanguageEditorService.enableLocalizedContent(payload, data => {
                dispatch({
                    type: ActionTypes.ENABLED_LOCALIZED_CONTENT,
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
    localizeContent(payload, callback, failureCallback) {
        return (dispatch) => {
            LanguageEditorService.localizeContent(payload, data => {
                dispatch({
                    type: ActionTypes.ENABLED_LOCALIZED_CONTENT,
                    data
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
    getLocalizationProgress(callback) {
        return () => {
            LanguageEditorService.getLocalizationProgress(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRoleGroups(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getRoleGroups(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLE_GROUPS,
                    data: {
                        roleGroups: data.Groups
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRoles(portalId, groupId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getRoles(portalId, groupId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLES,
                    data: {
                        rolesList: data.Roles
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPageList(payload, callback) {
        return (dispatch) => {
            LanguageEditorService.getPageList(payload, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_PAGES,
                    data: {
                        pageList: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    SelectLanguageRoles(roles, role, selected) {
        return (dispatch) => {
            let list = roles.map((item) => {
                if (item.RoleName === role) {
                    return { RoleID: item.RoleID, RoleName: item.RoleName, Selected: selected };
                }
                else {
                    return item;
                }
            });

            dispatch({
                type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLE_SELECTION,
                data: {
                    rolesList: list
                }
            });

        };
    },
    activateLanguage(payload, callback) {
        return () => {
            ApplicationService.activateLanguage(payload, () => {
                if (callback) {
                    callback();
                }
            });
        };
    },
    markAllPagesAsTranslated(payload, callback) {
        return () => {
            ApplicationService.markAllPagesAsTranslated(payload, () => {
                if (callback) {
                    callback();
                }
            });
        };
    }
};

export default languagesActions;
