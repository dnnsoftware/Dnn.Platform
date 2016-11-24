import ActionTypes from "../constants/actionTypes/languagesActionTypes";

export default function languages(state = {}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS:
            return { ...state,
                languageSettings: action.data.languageSettings
            };
        default:
            return state;
    }
}
