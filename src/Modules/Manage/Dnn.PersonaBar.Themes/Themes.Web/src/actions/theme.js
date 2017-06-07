import { theme as ActionTypes } from "constants/actionTypes";
import ThemeService from "services/themeService";
import Localization from "localization";

function errorCallback(xhr) {
    let response = eval("(" + xhr.responseText + ")");
    let message = xhr.responseText;
    if (response && response.Message) {
        message = response.Message;
    }

    let utils = window.dnn.initThemes().utility;
    utils.notifyError(Localization.get(message), 5000);
}
const themeActions = {
    getCurrentTheme(callback) {
        return (dispatch) => {
            ThemeService.getCurrentTheme(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CURRENT_THEMES,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getThemes(level, callback) {
        return (dispatch) => {
            ThemeService.getThemes(level, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEMES,
                    data: {
                        layouts: data.Layouts,
                        containers: data.Containers
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getCurrentThemeFiles(themeName, themeType, themeLevel, callback) {
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CURRENT_THEMEFILES,
                    data: {
                        themeFiles: data,
                        themeType: themeType
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableThemeFiles(themeName, themeType, themeLevel, callback) {
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_THEMEFILES,
                    data: {
                        themeFiles: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableTokens(callback) {
        return (dispatch) => {
            ThemeService.getEditableTokens(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_TOKENS,
                    data: {
                        tokens: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableSettings(token, callback) {
        return (dispatch) => {
            ThemeService.getEditableSettings(token, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_SETTINGS,
                    data: {
                        settings: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableValues(token, setting, callback) {
        return (dispatch) => {
            ThemeService.getEditableValues(token, setting, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_VALUES,
                    data: {
                        values: data.Value
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    applyTheme(themeFile, scope, callback) {
        return (dispatch) => {
            ThemeService.applyTheme(themeFile, scope, data => {
                dispatch({
                    type: ActionTypes.APPLY_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    updateTheme(path, token, setting, value, callback) {
        return (dispatch) => {
            ThemeService.updateTheme(path, token, setting, value, () => {
                dispatch({
                    type: ActionTypes.UPDATE_THEME,
                    data: {
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    parseTheme(themeName, parseType, callback) {
        return (dispatch) => {
            ThemeService.parseTheme(themeName, parseType, () => {
                dispatch({
                    type: ActionTypes.PARSE_THEME,
                    data: {
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    restoreTheme(callback) {
        return (dispatch) => {
            ThemeService.restoreTheme(data => {
                dispatch({
                    type: ActionTypes.RESTORE_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    applyDefaultTheme(themeName, level, callback) {
        return (dispatch) => {
            ThemeService.applyDefaultTheme(themeName, level, data => {
                dispatch({
                    type: ActionTypes.APPLY_DEFAULT_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    deleteTheme(theme, callback) {
        return (dispatch) => {
            ThemeService.deleteTheme(theme, () => {
                dispatch({
                    type: ActionTypes.DELETE_THEME,
                    data: {
                        theme: theme
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default themeActions;
