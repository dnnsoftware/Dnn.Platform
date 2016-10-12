import {siteInfo as ActionTypes, pagination as PaginationActionTypes}  from "../constants/actionTypes";

export default function siteInfoSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITEINFO_PORTAL_SETTINS:
            return { ...state,
                settings: action.data.Settings,
                timeZones: action.data.TimeZones,
                iconSets: action.data.IconSets
            };        
        default:
            return { ...state
            };
    }
}
