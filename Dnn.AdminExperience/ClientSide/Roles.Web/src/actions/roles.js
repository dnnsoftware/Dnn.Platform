import {roles as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

function errorCallback(message) {
    util.utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
}
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
            }, errorCallback);
        };
    },
    saveRoleGroup(group, callback) {
        return (dispatch) => {
            ApplicationService.saveRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLEGROUP,
                    data: { roleGroup: data }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteRoleGroup(group, callback) {
        return (dispatch) => {
            ApplicationService.deleteRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLEGROUP,
                    data: { groupId: data.groupId }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getRolesList(parameters, callback) {
        return (dispatch) => {
            ApplicationService.getRoles(parameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_ROLES_LIST,
                    data: { rolesList: data.roles },
                    loadMore: data.loadMore,
                    reload: parameters.reload,
                    rsvpLink: data.rsvpLink
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    saveRole(currentGroupId, assignExistUsers, role, callback) {
        return (dispatch) => {
            ApplicationService.saveRole(assignExistUsers, role, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLE,
                    data: { roleDetails: data, currentGroupId: currentGroupId }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteRole(role, callback) {
        return (dispatch) => {
            ApplicationService.deleteRole(role, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLE,
                    data: { roleId: data.roleId }
                });

                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default rolesActions;
