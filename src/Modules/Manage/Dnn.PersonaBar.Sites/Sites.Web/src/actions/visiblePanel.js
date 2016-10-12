import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel) {
        return dispatch => {
            dispatch({
                type: ActionTypes.SELECT_PANEL,
                payload: {
                    selectedPage: panel
                }
            });
        };
    }
};

export default visiblePanelActions;
