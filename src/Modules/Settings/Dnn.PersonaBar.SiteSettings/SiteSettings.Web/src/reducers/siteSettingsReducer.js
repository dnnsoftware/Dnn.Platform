import {siteSettings as ActionTypes, pagination as PaginationActionTypes}  from "../constants/actionTypes";

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
        default:
            return { ...state
            };
    }
}
