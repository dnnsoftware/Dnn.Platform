import PageSettingsActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";


const visiblePageSettingsActions = {

    showCustomPageSettings(pageSettingsId) {    
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.SHOW_CUSTOM_PAGE_SETTINGS,
                data: { pageSettingsId }
            });
        };
    },
    hideCustomPageSettings() {
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.HIDE_CUSTOM_PAGE_SETTINGS,
                data: {}
            });
        };       
    }
};
export default visiblePageSettingsActions
;