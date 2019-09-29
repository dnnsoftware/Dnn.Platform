import {
    siteInfo as ActionTypes
} from "../constants/actionTypes";

export default function siteInfo(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.CHANGED_PORTAL_ID:
            return {
                ...state,
                portalId: action.data.portalId
            };
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
        case ActionTypes.RETRIEVED_PORTALS:
            return { ...state,
                portals: action.data.portals,
                portalId: action.data.portals[0].PortalID
            };
        default:
            return state;
    }
}
