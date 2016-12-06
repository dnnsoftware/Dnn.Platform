import {
    search as ActionTypes
} from "../constants/actionTypes";

export default function search(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_BASIC_SEARCH_SETTINGS:
            return { ...state,
                basicSearchSettings: action.data.basicSearchSettings,
                searchCustomAnalyzers: action.data.searchCustomAnalyzers
            };
        case ActionTypes.SITESETTINGS_BASIC_SEARCH_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                basicSearchSettings: action.data.basicSearchSettings,
                basicSearchSettingsClientModified: action.data.basicSearchSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_BASIC_SEARCH_SETTINGS:
            return { ...state,
                basicSearchSettingsClientModified: action.data.basicSearchSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_SYNONYMS_GROUPS:
            return { ...state,
                synonymsGroups: action.data.synonymsGroups
            };
        case ActionTypes.CREATED_SITESETTINGS_SYNONYMS_GROUP:
        case ActionTypes.UPDATED_SITESETTINGS_SYNONYMS_GROUP:
            return { ...state,
                synonymsGroups: action.data.synonymsGroups,
                synonymsGroupClientModified: action.data.synonymsGroupClientModified
            };
        case ActionTypes.DELETED_SITESETTINGS_SYNONYMS_GROUP:
            return { ...state,
                synonymsGroups: action.data.synonymsGroups
            };
        case ActionTypes.SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED:
            return { ...state,
                synonymsGroupClientModified: action.data.synonymsGroupClientModified
            };
        case ActionTypes.CANCELED_SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED:
            return { ...state,
                synonymsGroupClientModified: action.data.synonymsGroupClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_IGNORE_WORDS:
            return { ...state,
                ignoreWords: action.data.ignoreWords
            };
        case ActionTypes.CREATED_SITESETTINGS_IGNORE_WORDS:
        case ActionTypes.UPDATED_SITESETTINGS_IGNORE_WORDS:
            return { ...state,
                ignoreWords: action.data.ignoreWords,
                ignoreWordsClientModified: action.data.ignoreWordsClientModified
            };
        case ActionTypes.SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED:
            return { ...state,
                ignoreWordsClientModified: action.data.ignoreWordsClientModified
            };
        case ActionTypes.CANCELED_SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED:
            return { ...state,
                ignoreWordsClientModified: action.data.ignoreWordsClientModified
            };
        case ActionTypes.DELETED_SITESETTINGS_IGNORE_WORDS:
            return { ...state,
                ignoreWords: action.data.ignoreWords
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_SEARCH_CULTURE_LIST:
            return { ...state,
                cultures: action.data.cultures
            };
        default:
            return state;
    }
}
