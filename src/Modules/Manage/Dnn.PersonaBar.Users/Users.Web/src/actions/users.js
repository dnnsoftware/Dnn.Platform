import {users as ActionTypes}  from "dnn-users-common-action-types";
import UserService from "services";
const userActions = {
    getUsers(searchParameters, callback) {
        return (dispatch) => {
            UserService.getUsers(searchParameters, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS,
                    payload
                });
                if (callback) {
                    callback(payload);
                }
            });
        };
    },
    getUserFilters(callback) {
        return () => {
            UserService.getUserFilters(payload => {
                // dispatch({
                //     type: ActionTypes.RETRIEVED_USER_FILTERS,
                //     payload
                // });
                if (callback) {
                    callback(payload);
                }
            });
        };
    }
};

export default userActions;
