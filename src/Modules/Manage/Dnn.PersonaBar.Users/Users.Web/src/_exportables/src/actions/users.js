import {users as ActionTypes}  from "../actionTypes";
import {CommonUsersService as UserService} from "../services";
import utilities from "utils";

function errorCallback(message) {
    utilities.notify(message);
}
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
            }, errorCallback);
        };
    },
    getUserDetails(userDetailsParameters, callback) {
        return (dispatch) => {
            UserService.getUserDetails(userDetailsParameters, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USER_DETAILS,
                    payload
                });
                if (callback) {
                    callback(payload);
                }
            }, errorCallback);
        };
    },
    getUserFilters(callback) {
        return () => {
            UserService.getUserFilters(payload => {
                if (callback) {
                    callback(payload);
                }
            }, errorCallback);
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
            }, errorCallback);
        };
    },
    updateUserBasicInfo(userDetails, callback) {
        return (dispatch) => {
            UserService.updateUserBasicInfo(userDetails, payload => {
                dispatch({
                    type: ActionTypes.UPDATE_USER,
                    payload
                });
                if (callback) {
                    callback(payload);
                }
            }, errorCallback);
        };
    },
    changePassword(payload, callback) {
        return () => {
            UserService.changePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    forceChangePassword(payload, callback) {
        return () => {
            UserService.forceChangePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    sendPasswordResetLink(payload, callback) {
        return () => {
            UserService.sendPasswordResetLink(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteUser(payload, callback) {
        return (dispatch) => {
            UserService.deleteUser(payload, data => {
                dispatch({
                    type: ActionTypes.DELETE_USER,
                    payload: { userId: payload.userId, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    eraseUser(payload, callback) {
        return (dispatch) => {
            UserService.hardDeleteUser(payload, data => {
                dispatch({
                    type: ActionTypes.ERASE_USER,
                    payload: { userId: payload.userId, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    restoreUser(payload, callback) {
        return (dispatch) => {
            UserService.restoreUser(payload, data => {
                dispatch({
                    type: ActionTypes.RESTORE_USER,
                    payload: { userId: payload.userId, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateSuperUserStatus(payload, callback) {
        return (dispatch) => {
            UserService.updateSuperUserStatus(payload, data => {
                dispatch({
                    type: ActionTypes.USER_MADE_SUPERUSER,
                    payload: { userId: payload.userId, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateAuthorizeStatus(payload, callback) {
        return (dispatch) => {
            UserService.updateAuthorizeStatus(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATE_USER_AUTHORIZE_STATUS,
                    payload: { userId: payload.userId, authorized: payload.authorized, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserRoles(searchParameters, callback) {
        return (dispatch) => {
            UserService.getUserRoles(searchParameters, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS_ROLES,
                    payload
                });
                if (callback) {
                    callback(payload);
                }
            }, errorCallback);
        };
    },
    getSuggestRoles(searchParameters, callback) {
        return (dispatch) => {
            UserService.getSuggestRoles(searchParameters, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUGGEST_ROLES,
                    payload: { matchedRoles: payload }
                });
                if (callback) {
                    callback(payload);
                }
            }, errorCallback);
        };
    },
    saveUserRole(payload, notifyUser, isOwner, callback) {
        return (dispatch) => {
            UserService.saveUserRole(payload, notifyUser, isOwner, data => {
                dispatch({
                    type: ActionTypes.SAVE_USER_ROLE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    removeUserRole(payload, callback) {
        return (dispatch) => {
            UserService.removeUserRole(payload, data => {
                dispatch({
                    type: ActionTypes.REMOVE_USER_ROLE,
                    payload: { userId: payload.userId, roleId: payload.roleId, Success: data.Success }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default userActions;
