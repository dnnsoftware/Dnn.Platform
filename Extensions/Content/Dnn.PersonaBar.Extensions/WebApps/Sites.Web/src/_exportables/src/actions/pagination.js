import {pagination as ActionTypes}  from "../actionTypes";
import utilities from "utils";
const paginationActions = {
    loadMore(callback) {
        return dispatch => {
            dispatch({
                type: ActionTypes.LOAD_MORE
            });
            if (callback) {
                utilities.throttleExecution(callback);
            }
        };
    }
};

export default paginationActions;
