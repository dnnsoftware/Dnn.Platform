import {users as ActionTypes}  from "../constants/actionTypes";
import UserService from "../services/userService";
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
    },
    createUser(userDetails, callback) {
        return (dispatch) => {
            UserService.createUser(userDetails, payload => {
                dispatch({
                    type: ActionTypes.CREATE_USER,
                    payload
                });
                if (callback) {
                    callback(payload);
                }
            });
        };
    }
};

export default userActions;
