import {theme as ActionTypes}  from "constants/actionTypes";
import ThemeService from "services/themeService";
function errorCallback(message) {
    let utils = window.dnn.initThemes().utility;
    utils.notify(message);
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
    getCurrentThemeFiles(themeName, themeType, themeLevel, callback) {
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CURRENT_THEMEFILES,
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
    getEditThemeFiles(themeName, themeType, themeLevel, callback) {
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDIT_THEMEFILES,
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
    }
};

export default themeActions;
