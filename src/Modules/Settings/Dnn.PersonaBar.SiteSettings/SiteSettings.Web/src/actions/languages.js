import { languages as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const languagesActions = {    
    getLanguageSettings(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getLanguageSettings(portalId, data => {
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
    updateLanguageSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateLanguageSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data: {
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
    getAllLanguages(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getAllLanguages(portalId, data => {
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
    }
};

export default languagesActions;
