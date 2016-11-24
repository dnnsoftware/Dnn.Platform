import { folder as ActionTypes} from "constants/actionTypes";
import { FolderService } from "services";
import utilities from "utils";

function errorCallback(message) {
    utilities.utilities.notifyError(message);
}
const folderActions = {
    getOwnerFolders(callback) {
        return (dispatch) => {
            FolderService.getOwnerFolders((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_OWNER_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getModuleFolders(type, callback) {
        return (dispatch) => {
            FolderService.getModuleFolders(type, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getModuleFiles(parameters, callback) {
        return (dispatch) => {
            FolderService.getModuleFiles(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createFolder(parameters, type, callback) {
        return (dispatch) => {
            FolderService.createNewFolder(parameters, () => {
                dispatch({
                    type: (type === "moduleFolder" ? ActionTypes.CREATED_NEW_MODULE_FOLDER : ActionTypes.CREATED_NEW_OWNER_FOLDER),
                    payload: {
                        value: parameters[type]
                    }
                });
                if (callback) {
                    callback(parameters);
                }
            }, errorCallback);
        };
    }
};

export default folderActions;
