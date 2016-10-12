import {users as ActionTypes}  from "constants/actionTypes";
import UserService from "services/userService";
const userActions = {
    getUsers(searchParameters, callback) {
        return (dispatch) => {
            UserService.getUsers(searchParameters, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS,
                    payload
                });
                if (callback) {
                    callback();
                }
            });
        };
    }
};

export default userActions;
