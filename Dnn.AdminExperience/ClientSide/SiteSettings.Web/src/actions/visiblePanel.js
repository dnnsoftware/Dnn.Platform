import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel, selectedPageVisibleIndex) {
        window.scrollTo(0, 0);
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
