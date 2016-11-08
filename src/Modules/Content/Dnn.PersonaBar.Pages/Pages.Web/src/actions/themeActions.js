import ActionTypes from "../constants/actionTypes/themeActionTypes";
import ThemeService from "../services/themeService";

const themeActions = {
    retrieveThemes() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.RETRIEVING_THEMES
            });    

            ThemeService.getThemes().then(response => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEMES,
                    data: {
                        themes: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_RETRIEVING_THEMES
                });
            });     
        };
    }
};
export default themeActions;