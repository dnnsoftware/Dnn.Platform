import {users as ActionTypes}  from "../actionTypes";
import {CommonUsersService as UserService} from "../services";
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
    getUserDetails(userDetailsParameters, callback) {
        return (dispatch) => {
            UserService.getUserDetails(userDetailsParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USER_DETAILS,
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
    },
    createUser(userDetails, filter, callback) {
        return (dispatch) => {
            UserService.createUser(userDetails, data => {
                dispatch({
                    type: ActionTypes.CREATE_USER,
                    payload: data,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateUserBasicInfo(userDetails, callback) {
        return (dispatch) => {
            UserService.updateUserBasicInfo(userDetails, data => {
                dispatch({
                    type: ActionTypes.UPDATE_USER,
                    payload: data
                });
                if (callback) {
                    callback(data);
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
    deleteUser(payload, filter, callback) {
        return (dispatch) => {
            let deletedUser = Object.assign({}, payload.userDetails);
            deletedUser.isDeleted = true;
            UserService.deleteUser({userId: payload.userDetails.userId}, data => {
                dispatch({
                    type: ActionTypes.DELETE_USER,
                    payload: deletedUser,
                    filter: filter
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
                    payload: { userId: payload.userId }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    restoreUser(payload, filter, callback) {
        return (dispatch) => {
            let restoredUser = Object.assign({}, payload.userDetails);
            restoredUser.isDeleted = false;
            UserService.restoreUser({userId: payload.userDetails.userId}, data => {
                dispatch({
                    type: ActionTypes.RESTORE_USER,
                    payload: restoredUser,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateSuperUserStatus(payload, filter, callback) {
        return (dispatch) => {
            UserService.updateSuperUserStatus(payload, data => {
                dispatch({
                    type: ActionTypes.USER_MADE_SUPERUSER,
                    payload: { userId: payload.userId, setSuperUser: payload.setSuperUser },
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateAuthorizeStatus(payload, authorized, filter, callback) {
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
            user.authorized = authorized;
            UserService.updateAuthorizeStatus({ userId: payload.userDetails.userId, authorized: authorized }, data => {
                dispatch({
                    type: ActionTypes.UPDATE_USER_AUTHORIZE_STATUS,
                    payload: user,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    unLockUser(payload, callback) {
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
            UserService.unlockUser({ userId: payload.userDetails.userId }, data => {
                dispatch({
                    type: ActionTypes.USER_UNLOCKED,
                    payload: user
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserRoles(searchParameters, callback) {
        return (dispatch) => {
            UserService.getUserRoles(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS_ROLES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    passwordStrength() {
        return (dispatch) => {
            UserService.passwordStrengthOptions(pStrength=>{
                dispatch({
                    type: ActionTypes.RETRIEVED_PASSWORD_STRENGTH_OPTIONS,
                    payload: pStrength
                });
            });
        };
    },
    getSuggestRoles(searchParameters, callback) {
        return (dispatch) => {
            UserService.getSuggestRoles(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUGGEST_ROLES,
                    payload: { matchedRoles: data }
                });
                if (callback) {
                    callback(data);
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
                    payload: { userId: payload.userId, roleId: payload.roleId }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default userActions;
