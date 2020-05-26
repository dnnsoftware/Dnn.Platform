import { security as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const securityActions = {
    getIpFilters(callback) {
        return (dispatch) => {
            ApplicationService.getIpFilters(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_IP_FILTERS,
                    data: {
                        ipFilters: data.Results.Filters,
                        enableIPChecking: data.Results.EnableIPChecking
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getIpFilter(searchParameters, callback) {
        return (dispatch) => {
            ApplicationService.getIpFilter(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_IP_FILTER,
                    data: {
                        ipFilter: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteIpFilter(filterId, ipFilters, callback) {
        return (dispatch) => {
            ApplicationService.deleteIpFilter(filterId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SECURITY_IP_FILTER,
                    data: {
                        ipFilters: ipFilters
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                util.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    getBasicLoginSettings(cultureCode, callback) {
        return (dispatch) => {
            ApplicationService.getBasicLoginSettings(cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_BASIC_LOGIN_SETTINS,
                    data: {
                        basicLoginSettings: data.Results.Settings,
                        adminUsers: data.Results.Administrators,
                        authProviders: data.Results.AuthProviders,
                        basicLoginSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateBasicLoginSettings(payload, callback) {
        return (dispatch) => {
            ApplicationService.updateBasicLoginSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_BASIC_LOGIN_SETTINS,
                    data: {
                        basicLoginSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    basicLoginSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_BASIC_LOGIN_SETTINS_CLIENT_MODIFIED,
                data: {
                    basicLoginSettings: parameter,
                    basicLoginSettingsClientModified: true
                }
            });
        };
    },
    memberSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_MEMBER_SETTINS_CLIENT_MODIFIED,
                data: {
                    memberSettings: parameter,
                    memberSettingsClientModified: true
                }
            });
        };
    },
    registrationSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_REGISTRATION_SETTINS_CLIENT_MODIFIED,
                data: {
                    registrationSettings: parameter,
                    registrationSettingsClientModified: true
                }
            });
        };
    },
    sslSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_SSL_SETTINS_CLIENT_MODIFIED,
                data: {
                    sslSettings: parameter,
                    sslSettingsClientModified: true
                }
            });
        };
    },
    otherSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_OTHER_SETTINS_CLIENT_MODIFIED,
                data: {
                    otherSettings: parameter,
                    otherSettingsClientModified: true
                }
            });
        };
    },
    updateIpFilter(payload, callback) {
        return (dispatch) => {
            ApplicationService.updateIpFilter(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_IP_FILTER,
                    data: {

                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getMemberSettings(callback) {
        return (dispatch) => {
            ApplicationService.getMemberSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_MEMBER_SETTINS,
                    data: {
                        memberSettings: data.Results.Settings,
                        memberSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateMemberSettings(payload, callback) {
        return (dispatch) => {
            ApplicationService.updateMemberSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_MEMBER_SETTINS,
                    data: {
                        memberSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRegistrationSettings(callback) {
        return (dispatch) => {
            ApplicationService.getRegistrationSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_REGISTRATION_SETTINS,
                    data: {
                        registrationSettings: data.Results.Settings,
                        userRegistrationOptions: data.Results.UserRegistrationOptions,
                        registrationFormTypeOptions: data.Results.RegistrationFormTypeOptions,
                        registrationSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateRegistrationSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateRegistrationSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_REGISTRATION_SETTINS,
                    data: {
                        registrationSettingsClientModified: false
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
    getSslSettings(callback) {
        return (dispatch) => {
            ApplicationService.getSslSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SSL_SETTINS,
                    data: {
                        sslSettings: data.Results.Settings,
                        sslSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSslSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateSslSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_SSL_SETTINS,
                    data: {
                        sslSettingsClientModified: false
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
    getOtherSettings(callback) {
        return (dispatch) => {
            ApplicationService.getOtherSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_OTHER_SETTINS,
                    data: {
                        otherSettings: data.Results.Settings,
                        otherSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateOtherSettings(payload, callback) {
        return (dispatch) => {
            ApplicationService.updateOtherSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_OTHER_SETTINS,
                    data: {
                        otherSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSecurityBulletins(callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.getSecurityBulletins(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_BULLETINS,
                    data: {
                        platformVersion: data.Results.PlatformVersion,
                        securityBulletins: data.Results.SecurityBulletins
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
    getSuperuserActivities(callback) {
        return (dispatch) => {
            ApplicationService.getSuperuserActivities(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SUPERUSER_ACTIVITIES,
                    data: {
                        activities: data.Results.Activities
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAuditCheckResults(callback) {
        return (dispatch) => {
            ApplicationService.getAuditCheckResults(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULTS,
                    data: {
                        auditCheckResults: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAuditCheckResult(id, callback) {
        return (dispatch) => {
            ApplicationService.getAuditCheckResult(id, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULT,
                    data: {
                        auditCheckResult: data.Result
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    searchFileSystemAndDatabase(searchParameters, callback) {
        return (dispatch) => {
            ApplicationService.searchFileSystemAndDatabase(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE,
                    data: {
                        searchResults: data.Results,
                        scannerCheckKeyword: searchParameters.term,
                        scannerCheckActiveTab: "search"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearFileSystemAndDatabaseSearch() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.RETRIEVED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE,
                data: {
                    searchResults: undefined,
                    scannerCheckKeyword: "",
                    scannerCheckActiveTab: "search"
                }
            });
        };
    },
    getLastModifiedSettings(callback) {
        return (dispatch) => {
            ApplicationService.getLastModifiedSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_SETTINGS,
                    data: {
                        modifiedSettings: data.Results,
                        scannerCheckActiveTab: "settings"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLastModifiedFiles(callback) {
        return (dispatch) => {
            ApplicationService.getLastModifiedFiles(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_FILES,
                    data: {
                        modifiedFiles: data.Results,
                        scannerCheckActiveTab: "files"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatefileSystemAndDatabaseSearchKeyword(keyword) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_KEYWORD,
                data: {
                    scannerCheckKeyword: keyword,
                    scannerCheckActiveTab: "search"
                }
            });
        };
    },
    updatefileSystemAndDatabaseActiveTab(tab) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_ACTIVE_TAB,
                data: {
                    scannerCheckActiveTab: tab
                }
            });
        };
    }
};

export default securityActions;
