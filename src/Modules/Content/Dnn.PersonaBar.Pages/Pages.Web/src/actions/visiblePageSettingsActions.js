import PageSettingsActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";


const visiblePanelActions = {
    //TODO: Verify the pageId to identify what custom detail panel should be preseted to user

    showCustomPageSettings() {        
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.SHOW_CUSTOM_PAGE_SETTINGS,
                data: {}
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
export default visiblePanelActions;