import { search as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

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
        return () => {
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
        return () => {
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
        return () => {
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
                        synonymsGroups: data
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
                let updatedGroups = groups.SynonymsGroups.map((item) => {
                    return item;
                });                
                updatedGroups.unshift({SynonymsGroupId: data.Id, SynonymsTags: payload.SynonymsTags});
                const synonymsGroups = Object.assign({}, groups);
                synonymsGroups.SynonymsGroups = updatedGroups;
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: synonymsGroups,
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
    deleteSynonymsGroup(groupId, object, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteSynonymsGroup(groupId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: object
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
    synonymsGroupClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED,
                data: {
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
                        ignoreWords: data
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
                let updated = Object.assign({}, payload);
                updated.StopWordsId =  data.Id;
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updated,
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
    ignoreWordsClientModified() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED,
                data: {
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
                let updated = Object.assign({}, payload);
                updated.StopWordsId =  -1;
                updated.StopWords = null;
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updated
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
    getCultureList(portalId, callback) {
        return (dispatch) => {
            ApplicationService.getCultureList(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_SEARCH_CULTURE_LIST,
                    data: {
                        cultures: data.Cultures
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }

};

export default searchActions;
