import {pagination as ActionTypes}  from "../constants/actionTypes";
const paginationActions = {
    loadTab(index) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_TAB_DATA,
                payload: {
                    index
                }
            });
        };
    }
};

export default paginationActions;
