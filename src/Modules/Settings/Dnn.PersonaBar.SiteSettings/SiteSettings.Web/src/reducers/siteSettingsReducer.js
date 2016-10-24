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
        default:
            return { ...state
            };
    }
}
