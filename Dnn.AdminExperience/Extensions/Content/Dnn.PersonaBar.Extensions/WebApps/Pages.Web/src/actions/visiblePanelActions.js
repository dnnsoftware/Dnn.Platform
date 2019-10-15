import ActionTypes from "../constants/actionTypes/visiblePanelActionTypes";

const visiblePanelActions = {    
    showPanel(panelId) {        
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SHOW_PANEL,
                data: {panelId}
            });
        };
    },
    hidePanel() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.HIDE_PANEL,
                data: {}
            });
        };       
    }
};
export default visiblePanelActions;