import {theme as ActionTypes}  from "../constants/actionTypes";
export default function theme(state = {
    currentTheme: {SiteLayout: {}, SiteContainer: {}, EditLayout: {}, EditContainer: {}},
    themes: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_CURRENT_THEMES:
            return { ...state,
                currentTheme: action.data.currentTheme
            };
        default:
            return { ...state
            };
    }
}
