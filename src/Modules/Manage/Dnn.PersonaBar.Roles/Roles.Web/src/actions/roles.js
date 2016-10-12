import {roles as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const rolesActions = {
    getRoleGroupsList(reload, callback) {
        return (dispatch) => {
            ApplicationService.getRoleGroups(reload, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_GROUPS_LIST,
                    data: { roleGroups: data }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    saveRoleGroup(group, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.saveRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLEGROUP,
                    data: { roleGroup: data }
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
    deleteRoleGroup(group, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLEGROUP,
                    data: { groupId: data.groupId }
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
    getRolesList(parameters, callback) {
        return (dispatch) => {
            ApplicationService.getRoles(parameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_ROLES_LIST,
                    data: { rolesList: data.roles },
                    loadMore: data.loadMore,
                    reload: parameters.reload
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    saveRole(currentGroupId, assignExistUsers, role, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.saveRole(assignExistUsers, role, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLE,
                    data: { roleDetails: data, currentGroupId: currentGroupId }
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
    deleteRole(role, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.deleteRole(role, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLE,
                    data: { roleId: data.roleId }
                });

                if (callback) {
                    callback();
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    }
};

export default rolesActions;
