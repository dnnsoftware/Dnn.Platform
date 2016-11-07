import { search as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const searchActions = {    
    getBasicSearchSettings(callback) {
        return (dispatch) => {
            ApplicationService.getBasicSearchSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_BASIC_SEARCH_SETTINGS,
                    data: {
                        basicSearchSettings: data.Settings,
                        searchCustomAnalyzers: data.SearchCustomAnalyzers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    basicSearchSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_BASIC_SEARCH_SETTINS_CLIENT_MODIFIED,
                data: {
                    basicSearchSettings: parameter,
                    basicSearchSettingsClientModified: true
                }
            });
        };
    },
    updateBasicSearchSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateBasicSearchSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_BASIC_SEARCH_SETTINGS,
                    data: {
                        basicSearchSettingsClientModified: false
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
    compactSearchIndex(callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.compactSearchIndex(data => {
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
    hostSearchReindex(callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.hostSearchReindex(data => {
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
    portalSearchReindex(portalId, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.portalSearchReindex(portalId, data => {
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
    getSynonymsGroups(portalId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getSynonymsGroups(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_SYNONYMS_GROUPS,
                    data: {
                        synonymsGroups: data.SynonymsGroups
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addSynonymsGroup(payload, groups, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addSynonymsGroup(payload, data => {
                let updatedGroups = groups.map((item, index) => {
                    return item;
                });                
                updatedGroups.unshift({SynonymsGroupId: data.Id, SynonymsTags: payload.SynonymsTags});

                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: updatedGroups,
                        synonymsGroupClientModified: false
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
    updateSynonymsGroup(payload, groups, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateSynonymsGroup(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: groups,
                        synonymsGroupClientModified: false
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
    deleteSynonymsGroup(groupId, groups, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteSynonymsGroup(groupId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: groups
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
    synonymsGroupClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED,
                data: {
                    synonymsGroup: parameter,
                    synonymsGroupClientModified: true
                }
            });
        };
    },
    cancelSynonymsGroupClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED,
                data: {
                    synonymsGroupClientModified: false
                }
            });
        };
    },
    getIgnoreWords(portalId, cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getIgnoreWords(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: data.IgnoreWords
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addIgnoreWords(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addIgnoreWords(payload, data => {
                let updatedWords = Object.assign({ StopWordsId: data.Id }, payload);

                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updatedWords,
                        ignoreWordsClientModified: false
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
    updateIgnoreWords(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateIgnoreWords(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: payload,
                        ignoreWordsClientModified: false
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
    ignoreWordsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED,
                data: {
                    ignoreWords: parameter,
                    ignoreWordsClientModified: true
                }
            });
        };
    },
    cancelIgnoreWordsClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED,
                data: {
                    ignoreWordsClientModified: false
                }
            });
        };
    },
    deleteIgnoreWords(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteIgnoreWords(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: undefined
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

export default searchActions;
