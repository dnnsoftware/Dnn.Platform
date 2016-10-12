import {viewMode as ActionTypes}  from "../constants/actionTypes";
const viewModeActions = {
    setCardView() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_CARD_VIEW
            });
        };
    },
    setListView() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_LIST_VIEW
            });
        };
    }
};

export default viewModeActions;
