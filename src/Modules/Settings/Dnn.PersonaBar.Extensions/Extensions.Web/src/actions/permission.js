import { permission as ActionTypes } from "constants/actionTypes";
import { PermissionService } from "services";
import utilities from "utils";

function errorCallback(message) {
    utilities.utilities.notifyError(message);
}
const permissionActions = {
    getDesktopModulePermissions(desktopModuleId, callback) {
        return (dispatch) => {
            PermissionService.getDesktopModulePermissions(desktopModuleId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_DESKTOPMODULE_PERMISSIONS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    saveDesktopModulePermissions(permissions, callback) {
        return (dispatch) => {
            PermissionService.saveDesktopModulePermissions(permissions, (data) => {
                dispatch({
                    type: ActionTypes.UPDATED_DESKTOPMODULE_PERMISSIONS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default permissionActions;
