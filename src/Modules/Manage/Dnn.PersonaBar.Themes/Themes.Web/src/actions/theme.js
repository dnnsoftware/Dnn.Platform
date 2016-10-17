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
    }
};

export default themeActions;
