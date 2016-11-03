import { siteSettings as ActionTypes, pagination as PaginationActionTypes } from "../constants/actionTypes";

export default function siteSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_SETTINGS:
            return { ...state,
                settings: action.data.settings,
                timeZones: action.data.timeZones,
                iconSets: action.data.iconSets,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SITESETTINGS_PORTAL_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                settings: action.data.settings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_PORTAL_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_DEFAULT_PAGES_SETTINGS:
            return { ...state,
                defaultPagesSettings: action.data.settings,
                defaultPagesSettingsClientModified: action.data.defaultPagesSettingsClientModified
            };
        case ActionTypes.SITESETTINGS_DEFAULT_PAGES_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                defaultPagesSettings: action.data.settings,
                defaultPagesSettingsClientModified: action.data.defaultPagesSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_DEFAULT_PAGES_SETTINGS:
            return { ...state,
                defaultPagesSettingsClientModified: action.data.defaultPagesSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_MESSAGING_SETTINGS:
            return { ...state,
                messagingSettings: action.data.settings,
                messagingSettingsClientModified: action.data.messagingSettingsClientModified
            };
        case ActionTypes.SITESETTINGS_MESSAGING_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                messagingSettings: action.data.settings,
                messagingSettingsClientModified: action.data.messagingSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_MESSAGING_SETTINGS:
            return { ...state,
                messagingSettingsClientModified: action.data.messagingSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_SETTINGS:
            return { ...state,
                profileSettings: action.data.settings,
                userVisibilityOptions: action.data.userVisibilityOptions,
                profileSettingsClientModified: action.data.profileSettingsClientModified
            };
        case ActionTypes.SITESETTINGS_PROFILE_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                profileSettings: action.data.settings,
                profileSettingsClientModified: action.data.profileSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_PROFILE_SETTINGS:
            return { ...state,
                profileSettingsClientModified: action.data.profileSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTIES:
            return { ...state,
                profileProperties: action.data.profileProperties
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY:
            return { ...state,
                profileProperty: action.data.profileProperty,
                userVisibilityOptions: action.data.userVisibilityOptions,
                dataTypeOptions: action.data.dataTypeOptions,
                languageOptions: action.data.languageOptions,
                profilePropertyClientModified: action.data.profilePropertyClientModified
            };
        case ActionTypes.SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED:
            return { ...state,
                profileProperty: action.data.profileProperty,
                profilePropertyClientModified: action.data.profilePropertyClientModified
            };
        case ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED:
            return { ...state,
                profilePropertyClientModified: action.data.profilePropertyClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION:
        case ActionTypes.SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED:
            return { ...state,
                propertyLocalization: action.data.propertyLocalization,
                propertyLocalizationClientModified: action.data.propertyLocalizationClientModified
            };
        case ActionTypes.CREATED_SITESETTINGS_PROFILE_PROPERTY:
        case ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY:
            return { ...state,
                profilePropertyClientModified: action.data.profilePropertyClientModified
            };
        case ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED:
            return { ...state,
                propertyLocalizationClientModified: action.data.propertyLocalizationClientModified
            };
        case ActionTypes.DELETED_SITESETTINGS_PROFILE_PROPERTY:
            return { ...state,
                profileProperties: action.data.profileProperties
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS_SETTINGS:
            return { ...state,
                urlMappingSettings: action.data.urlMappingSettings,
                portalAliasMappingModes: action.data.portalAliasMappingModes,
                urlMappingSettingsClientModified: action.data.urlMappingSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIASES:
            return { ...state,
                siteAliases: action.data.siteAliases,
                browsers: action.data.browsers,
                languages: action.data.languages,
                skins: action.data.skins
            };
        case ActionTypes.SITESETTINGS_URL_MAPPING_SETTINGS_CLIENT_MODIFIED:
            return { ...state,
                urlMappingSettings: action.data.urlMappingSettings,
                urlMappingSettingsClientModified: action.data.urlMappingSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_URL_MAPPING_SETTINGS:
            return { ...state,
                urlMappingSettingsClientModified: action.data.urlMappingSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS:
            return { ...state,
                aliasDetail: action.data.aliasDetail
            };
        case ActionTypes.CANCELED_SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED:
            return { ...state,
                siteAliasClientModified: action.data.siteAliasClientModified
            };
        case ActionTypes.SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED:
            return { ...state,
                aliasDetail: action.data.aliasDetail,
                siteAliasClientModified: action.data.siteAliasClientModified
            };
        case ActionTypes.CREATED_SITESETTINGS_SITE_ALIAS:
        case ActionTypes.UPDATED_SITESETTINGS_SITE_ALIAS:
            return { ...state,
                siteAliasClientModified: action.data.siteAliasClientModified
            };
        case ActionTypes.DELETED_SITESETTINGS_SITE_ALIAS:
            return { ...state,
                siteAliases: action.data.siteAliases
            };
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
                synonymsGroup: action.data.synonymsGroup,
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
                ignoreWords: action.data.ignoreWords,
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
        default:
            return state;
    }
}
