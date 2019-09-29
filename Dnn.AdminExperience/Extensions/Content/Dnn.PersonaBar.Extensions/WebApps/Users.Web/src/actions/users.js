import {users as ActionTypes}  from "dnn-users-common-action-types";
import UserService from "services";
import utilities from "utils";

function errorCallback(message) {
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
}
const userActions = {
    getUsers(searchParameters, callback) {
        return (dispatch) => {
            UserService.getUsers(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserFilters(callback) {
        return () => {
            UserService.getUserFilters(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default userActions;
