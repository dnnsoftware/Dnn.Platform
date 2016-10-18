import {seo as ActionTypes, pagination as PaginationActionTypes}  from "../constants/actionTypes";

export default function seoSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS:
            return { ...state,
                generalSettings: action.data.generalSettings,
                replacementCharacterList: action.data.replacementCharacterList,
                deletedPageHandlingTypes: action.data.deletedPageHandlingTypes,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                generalSettings: action.data.generalSettings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SEO_GENERAL_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        case ActionTypes.RETRIEVED_SEO_REGEX_SETTINGS:
            return { ...state,
                regexSettings: action.data.regexSettings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SEO_REGEX_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                regexSettings: action.data.regexSettings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SEO_REGEX_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        default:
            return { ...state
            };
    }
}
