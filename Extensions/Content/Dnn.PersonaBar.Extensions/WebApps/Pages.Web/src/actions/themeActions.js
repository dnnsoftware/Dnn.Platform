import ActionTypes from "../constants/actionTypes/themeActionTypes";
import ThemeService from "../services/themeService";

const themeActions = {
    retrieveThemes() {
        return (dispatch) => new Promise(function(callback) {
            dispatch({
                type: ActionTypes.RETRIEVING_THEMES
            });    

            ThemeService.getThemes().then(response => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEMES,
                    data: {
                        themes: response.themes,
                        defaultPortalThemeName: response.defaultPortalThemeName,
                        defaultPortalThemeLevel: response.defaultPortalThemeLevel,
                        defaultPortalLayout: response.defaultPortalLayout,
                        defaultPortalContainer: response.defaultPortalContainer
                    }
                });
                callback({success: true});
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_RETRIEVING_THEMES,
                    data: {error}
                });
                callback({success: false});
            });     
        });
    },

    retrieveThemeFiles(themeName, level) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.RETRIEVING_THEME_FILES
            });    

            ThemeService.getThemeFiles(themeName, level).then(response => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEME_FILES,
                    data: {
                        layouts: response.layouts,
                        containers: response.containers
                    }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_RETRIEVING_THEME_FILES,
                    data: {error}
                });
            });     
        };
    }
};
export default themeActions;