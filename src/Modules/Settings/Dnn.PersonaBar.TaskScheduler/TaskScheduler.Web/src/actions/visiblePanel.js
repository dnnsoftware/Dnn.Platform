import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel, selectedPageVisibleIndex) {
        return dispatch => {
            dispatch({
                type: ActionTypes.SELECT_PANEL,
                payload: {
                    selectedPage: panel,
                    selectedPageVisibleIndex: selectedPageVisibleIndex
                }
            });
        };
    }
};

export default visiblePanelActions;
