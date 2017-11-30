import {
    languages as ActionTypes
} from "../constants/actionTypes";

export default function languages(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS:
            return { ...state,
                languageSettings: action.data.languageSettings,
                languages: action.data.languages,
                languageDisplayModes: action.data.languageDisplayModes,
                languageSettingsClientModified: action.data.languageSettingsClientModified
            };
        case ActionTypes.SITESETTINGS_LANGUAGE_SETTINGS_CLIENT_MODIFIED:
            return { ...state,
                languageSettings: action.data.languageSettings,
                languageSettingsClientModified: action.data.languageSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_SETTINGS:
            return { ...state,
                languageList: action.data.languageList.map((language) => {
                    return { 
                        ...language,
                        IsDefault: action.data.siteDefaultLanguage === language.Code
                    };
                }),
                languageSettingsClientModified: action.data.languageSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGES:
            return { ...state,
                languageList: action.data.languageList
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE:
            return { ...state,
                language: action.data.language,
                fallbacks: action.data.fallbacks,
                languageClientModified: action.data.languageClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_ALL_LANGUAGES:
            return { ...state,
                fullLanguageList: action.data.fullLanguageList
            };
        case ActionTypes.SITESETTINGS_LANGUAGE_CLIENT_MODIFIED:
            return { ...state,
                language: action.data.language,
                languageClientModified: action.data.languageClientModified
            };
        case ActionTypes.CANCELED_SITESETTINGS_LANGUAGE_CLIENT_MODIFIED:
            return { ...state,
                languageClientModified: action.data.languageClientModified
            };
        case ActionTypes.CREATED_SITESETTINGS_LANGUAGE:
            return { ...state,
                languageClientModified: action.data.languageClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_LANGUAGE:
        case ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLES:
            return { ...state,
                languageClientModified: action.data.languageClientModified
            };
        case ActionTypes.VERIFIED_SITESETTINGS_LANGUAGE_RESOURCE_FILES:
            return { ...state,
                verificationResults: action.data.verificationResults
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_PACK_MODULE_LIST:
            return { ...state,
                modules: action.data.modules
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLE_GROUPS:
            return { ...state,
                roleGroups: action.data.roleGroups
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLES:
            return { ...state,
                rolesList: action.data.rolesList
            };
        case ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLE_SELECTION:
            return { ...state,
                rolesList: action.data.rolesList
            };
        default:
            return state;
    }
}
