import {seo as ActionTypes, pagination as PaginationActionTypes}  from "../constants/actionTypes";

export default function seoSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS:
            return { ...state,
                settings: action.data.settings,
                replacementCharacterList: action.data.replacementCharacterList,
                deletedPageHandlingTypes: action.data.deletedPageHandlingTypes,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                settings: action.data.settings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SEO_GENERAL_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        default:
            return { ...state
            };
    }
}
