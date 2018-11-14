import { 
    siteBehavior as ActionTypes
} from "../constants/actionTypes";

export default function siteBehavior(state = {
}, action) {
    switch (action.type) {        
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
        case ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER:
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
                siteAliases: action.data.siteAliases
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
        case ActionTypes.RETRIEVED_SITESETTINGS_OTHER_SETTINGS:
            return { ...state,
                otherSettings: action.data.settings,
                otherSettingsClientModified: action.data.otherSettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_OTHER_SETTINGS:
        case ActionTypes.CANCELED_SITESETTINGS_OTHER_SETTINGS_CLIENT_MODIFIED:
            return { ...state,
                otherSettingsClientModified: action.data.otherSettingsClientModified
            };
        case ActionTypes.SITESETTINGS_OTHER_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                otherSettings: action.data.settings,
                otherSettingsClientModified: action.data.otherSettingsClientModified
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_LIST_INFO: 
            return { ...state,
                enableSortOrder: action.data.enableSortOrder,
                entries: action.data.entries
            };
        case ActionTypes.DELETED_SITESETTINGS_LIST_ENTRY:
            return { ...state,
                entries: action.data.entries
            };
        case ActionTypes.UPDATED_SITESETTINGS_LIST_ENTRY_ORDER:
            return { ...state,
                entries: action.data.entries
            };
        case ActionTypes.RETRIEVED_SITESETTINGS_PRIVACY_SETTINGS:
            return { ...state,
                privacySettings: action.data.settings,
                privacySettingsClientModified: action.data.privacySettingsClientModified
            };
        case ActionTypes.SITESETTINGS_PRIVACY_SETTINGS_CLIENT_MODIFIED:
            return { ...state,
                privacySettings: action.data.settings,
                privacySettingsClientModified: action.data.privacySettingsClientModified
            };
        case ActionTypes.UPDATED_SITESETTINGS_PRIVACY_SETTINGS:
            return { ...state,
                privacySettingsClientModified: action.data.privacySettingsClientModified
            };
        default:
            return state;
    }
}
