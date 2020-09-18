import { theme as ActionTypes } from "../constants/actionTypes";
export default function theme(state = {
    currentTheme: { SiteLayout: {}, SiteContainer: {}, EditLayout: {}, EditContainer: {} },
    themes: { layouts: [], containers: [] },
    currentThemeFiles: [[], []],
    editableThemeFiles: [],
    editableTokens: [],
    editableSettings: [],
    editableValue: ""
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_CURRENT_THEMES:
            return { ...state,
                currentTheme: action.data.currentTheme
            };
        case ActionTypes.RETRIEVED_CURRENT_THEMEFILES: {
            let currentThemeFiles = Object.assign([], state.currentThemeFiles);
            currentThemeFiles[action.data.themeType] = action.data.themeFiles;
            return { ...state,
                currentThemeFiles: currentThemeFiles
            };
        }
        case ActionTypes.APPLY_THEME:
            return { ...state,
                currentTheme: action.data.currentTheme
            };
        case ActionTypes.RETRIEVED_THEMES:
            return { ...state,
                themes: { layouts: action.data.layouts, containers: action.data.containers }
            };
        case ActionTypes.RETRIEVED_EDITABLE_THEMEFILES:
            return { ...state,
                editableThemeFiles: action.data.themeFiles
            };
        case ActionTypes.RETRIEVED_EDITABLE_TOKENS:
            return { ...state,
                editableTokens: action.data.tokens
            };
        case ActionTypes.RETRIEVED_EDITABLE_SETTINGS:
            return { ...state,
                editableSettings: action.data.settings
            };
        case ActionTypes.RETRIEVED_EDITABLE_VALUES:
            return { ...state,
                editableValue: action.data.values
            };
        case ActionTypes.UPDATE_THEME:
            return { ...state };
        case ActionTypes.PARSE_THEME:
            return { ...state };
        case ActionTypes.RESTORE_THEME:
            return { ...state,
                currentTheme: action.data.currentTheme,
                currentThemeFiles: [[], []]
            };
        case ActionTypes.APPLY_DEFAULT_THEME:
            return { ...state,
                currentTheme: action.data.currentTheme,
                currentThemeFiles: [[], []]
            };
        case ActionTypes.DELETE_THEME:
        {
            let deletedTheme = action.data.theme;
            let layouts = state.themes.layouts;
            if (layouts.some(l => l.packageName === deletedTheme.packageName)) {
                layouts = layouts.filter(l => {
                    return l.packageName !== deletedTheme.packageName;
                });
            }

            let containers = state.themes.containers;
            if (containers.some(l => l.packageName === deletedTheme.packageName)) {
                containers = containers.filter(l => {
                    return l.packageName !== deletedTheme.packageName;
                });
            }

            return { ...state,
                themes: { layouts: layouts, containers: containers }
            };
        }
        default:
            return { ...state
            };
    }
}
