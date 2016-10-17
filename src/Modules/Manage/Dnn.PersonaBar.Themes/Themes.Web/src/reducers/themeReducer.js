import {theme as ActionTypes}  from "../constants/actionTypes";
export default function theme(state = {
    currentTheme: {SiteLayout: {}, SiteContainer: {}, EditLayout: {}, EditContainer: {}},
    themes: [],
    currentThemeFiles: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_CURRENT_THEMES:
            return { ...state,
                currentTheme: action.data.currentTheme
            };
        case ActionTypes.RETRIEVED_CURRENT_THEMEFILES:
            return { ...state,
                currentThemeFiles: action.data.themeFiles
            };
        default:
            return { ...state
            };
    }
}
