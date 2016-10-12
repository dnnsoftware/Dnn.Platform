import {roleUsers as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

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
            });
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
            });
        };
    },
    addUserToRole(parameters, notifyUser, isOwner, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.addUserToRole(parameters, notifyUser, isOwner, (data) => {
                dispatch({
                    type: ActionTypes.ADD_USER_INTO_ROLE,
                    data: { roleUserDetails: data, isAdd: parameters.isAdd }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    removeUserFromRole(parameters, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.removeUserFromRole(parameters, data => {
                dispatch({
                    type: ActionTypes.REMOVE_USER,
                    data: { UserId: data.UserId, RoleId: data.RoleId }
                });

                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    }

};

export default roleUsersActions;
