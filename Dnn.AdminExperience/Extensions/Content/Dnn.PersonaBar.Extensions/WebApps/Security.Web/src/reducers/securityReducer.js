import { security as ActionTypes } from "../constants/actionTypes";

export default function securitySettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SECURITY_BASIC_LOGIN_SETTINS:
            return { ...state,
                basicLoginSettings: action.data.basicLoginSettings,
                adminUsers: action.data.adminUsers,
                authProviders: action.data.authProviders,
                basicLoginSettingsClientModified: false
            };
        case ActionTypes.RETRIEVED_SECURITY_MEMBER_SETTINS:
            return { ...state,
                memberSettings: action.data.memberSettings,
                memberSettingsClientModified: false
            };
        case ActionTypes.RETRIEVED_SECURITY_OTHER_SETTINS:
            return { ...state,
                otherSettings: action.data.otherSettings,
                otherSettingsClientModified: false
            };
        case ActionTypes.RETRIEVED_SECURITY_SSL_SETTINS:
            return { ...state,
                sslSettings: action.data.sslSettings,
                sslSettingsClientModified: false
            };
        case ActionTypes.RETRIEVED_SECURITY_REGISTRATION_SETTINS:
            return { ...state,
                registrationSettings: action.data.registrationSettings,
                userRegistrationOptions: action.data.userRegistrationOptions,
                registrationFormTypeOptions: action.data.registrationFormTypeOptions,
                registrationSettingsClientModified: false
            };
        case ActionTypes.RETRIEVED_SECURITY_IP_FILTERS:
            return { ...state,
                ipFilters: action.data.ipFilters,
                enableIPChecking: action.data.enableIPChecking
            };
        case ActionTypes.DELETED_SECURITY_IP_FILTER:
            return { ...state,
                ipFilters: action.data.ipFilters
            };
        case ActionTypes.RETRIEVED_SECURITY_IP_FILTER:
            return { ...state,
                ipFilter: action.data.ipFilter
            };
        case ActionTypes.RETRIEVED_SECURITY_BULLETINS:
            return { ...state,
                platformVersion: action.data.platformVersion,
                securityBulletins: action.data.securityBulletins
            };
        case ActionTypes.RETRIEVED_SECURITY_SUPERUSER_ACTIVITIES:
            return { ...state,
                activities: action.data.activities
            };
        case ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULTS:
            return { ...state,
                auditCheckResults: action.data.auditCheckResults
            };
        case ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULT:
        {
            let result = action.data.auditCheckResult;
            let curentResults = Object.assign([], JSON.parse(JSON.stringify(state.auditCheckResults)));
            for (let i = 0; i < curentResults.length; i++) {
                if (curentResults[i].CheckName === result.CheckName) {
                    curentResults[i] = result;
                    break;
                }
            }
            return { ...state,
                auditCheckResults: curentResults
            };
        }
        case ActionTypes.RETRIEVED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE:
            return { ...state,
                searchResults: action.data.searchResults,
                scannerCheckKeyword: action.data.scannerCheckKeyword,
                scannerCheckActiveTab: action.data.scannerCheckActiveTab
            };
        case ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_SETTINGS:
            return { ...state,
                modifiedSettings: action.data.modifiedSettings,
                scannerCheckActiveTab: action.data.scannerCheckActiveTab
            };
        case ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_FILES:
            return { ...state,
                modifiedFiles: action.data.modifiedFiles,
                scannerCheckActiveTab: action.data.scannerCheckActiveTab
            };
        case ActionTypes.SECURITY_BASIC_LOGIN_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                basicLoginSettings: action.data.basicLoginSettings,
                basicLoginSettingsClientModified: action.data.basicLoginSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_BASIC_LOGIN_SETTINS:
            return { ...state,
                basicLoginSettingsClientModified: action.data.basicLoginSettingsClientModified
            };
        case ActionTypes.SECURITY_MEMBER_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                memberSettings: action.data.memberSettings,
                memberSettingsClientModified: action.data.memberSettingsClientModified
            };
        case ActionTypes.SECURITY_REGISTRATION_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                registrationSettings: action.data.registrationSettings,
                registrationSettingsClientModified: action.data.registrationSettingsClientModified
            };
        case ActionTypes.SECURITY_SSL_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                sslSettings: action.data.sslSettings,
                sslSettingsClientModified: action.data.sslSettingsClientModified
            };
        case ActionTypes.SECURITY_OTHER_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                otherSettings: action.data.otherSettings,
                otherSettingsClientModified: action.data.otherSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_MEMBER_SETTINS:
            return { ...state,
                memberSettingsClientModified: action.data.memberSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_REGISTRATION_SETTINS:
            return { ...state,
                registrationSettingsClientModified: action.data.registrationSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_SSL_SETTINS:
            return { ...state,
                sslSettingsClientModified: action.data.sslSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_OTHER_SETTINS:
            return { ...state,
                otherSettingsClientModified: action.data.otherSettingsClientModified
            };
        case ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_KEYWORD:
            return { ...state,
                scannerCheckActiveTab: action.data.scannerCheckActiveTab,
                scannerCheckKeyword: action.data.scannerCheckKeyword
            };
        case ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_ACTIVE_TAB:
            return { ...state,
                scannerCheckActiveTab: action.data.scannerCheckActiveTab
            };
        default:
            return { ...state
            };
    }
}
