import {roleUsers as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

function errorCallback(message) {
    util.utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
}

const roleUsersActions = {
    getSuggestUsers(parameters, callback) {
        return (dispatch) => {
            ApplicationService.getSuggestUsers(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUGGEST_LIST,
                    data: { matchedUsers: data }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getRoleUsers(parameters, callback) {
        return (dispatch) => {
            ApplicationService.getRoleUsers(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS_LIST,
                    data: { roleUsers: data.users, totalRecords: data.totalRecords }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    addUserToRole(parameters, notifyUser, isOwner, callback) {
        return (dispatch) => {
            ApplicationService.addUserToRole(parameters, notifyUser, isOwner, (data) => {
                dispatch({
                    type: ActionTypes.ADD_USER_INTO_ROLE,
                    data: { roleUserDetails: data, isAdd: parameters.isAdd }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    removeUserFromRole(parameters, callback) {
        return (dispatch) => {
            ApplicationService.removeUserFromRole(parameters, data => {
                dispatch({
                    type: ActionTypes.REMOVE_USER,
                    data: { UserId: data.UserId, RoleId: data.RoleId }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }

};

export default roleUsersActions;
