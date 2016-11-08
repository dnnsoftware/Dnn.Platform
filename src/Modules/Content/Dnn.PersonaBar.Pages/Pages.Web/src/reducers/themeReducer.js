import ActionTypes  from "../constants/actionTypes/themeActionTypes";
export default function themeReducer(state = {
    themes: [],
    retrievedThemes: false,
    retrievingThemes: false
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVING_THEMES:
            return { ...state,
                retrievingThemes: true
            };

        case ActionTypes.RETRIEVED_THEMES:
            return { ...state,
                themes: action.data.themes,
                retrievedThemes: true,
                retrievingThemes: false
            };
        
        case ActionTypes.ERROR_RETRIEVING_THEMES:
            return { ...state,
                retrievingThemes: false
            };

        default:
            return state;
    }
}
