import ActionTypes  from "../constants/actionTypes/themeActionTypes";
export default function themeReducer(state = {
    themes: [],
    layouts: [],
    containers: [],
    retrievedThemes: false,
    retrievingThemes: false,
    retrievingThemeFiles: false
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
            
        case ActionTypes.RETRIEVING_THEME_FILES:
            return { ...state,
                retrievingThemeFiles: true
            };

        case ActionTypes.RETRIEVED_THEME_FILES:
            return { ...state,
                layouts: action.data.layouts,
                containers: action.data.containers,
                retrievingThemeFiles: false
            };
        
        case ActionTypes.ERROR_RETRIEVING_THEME_FILES:
            return { ...state,
                retrievingThemeFiles: false
            };

        default:
            return state;
    }
}
