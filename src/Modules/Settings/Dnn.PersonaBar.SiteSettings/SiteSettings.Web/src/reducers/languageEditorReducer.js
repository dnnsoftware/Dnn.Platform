import {languageEditor as ActionTypes}  from "../constants/actionTypes";
export default function languageEditor(state = {
    languageBeingEdited: {}
}, action) {
    switch (action.type) {
        case ActionTypes.SET_LANGUAGE_BEING_EDITED:
            return { ...state,
                languageBeingEdited: action.payload
            };
        default:
            return { ...state
            };
    }
}
