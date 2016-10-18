import {theme as ActionTypes}  from "../constants/actionTypes";
export default function theme(state = {
    currentTheme: {SiteLayout: {}, SiteContainer: {}, EditLayout: {}, EditContainer: {}},
    themes: {layouts: [], containers: []},
    currentThemeFiles: [],
    editThemeFiles: []    
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
        case ActionTypes.RETRIEVED_EDIT_THEMEFILES:
            return { ...state,
                editThemeFiles: action.data.themeFiles
            };
        case ActionTypes.APPLY_THEME:
            return { ...state,
                currentTheme: action.data.currentTheme
            };
        case ActionTypes.RETRIEVED_THEMES:
            return { ...state,
                themes: {layouts: action.data.layouts, containers: action.data.containers}
            };
        default:
            return { ...state
            };
    }
}
