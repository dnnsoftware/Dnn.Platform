import { folder as ActionTypes } from "constants/actionTypes";
import { FolderService } from "services";
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
            });
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
            });
        };
    },
    createFolder(parameters, type, callback) {
        return (dispatch) => {
            FolderService.createNewFolder(parameters, (data) => {
                dispatch({
                    type: (type === "moduleFolder" ? ActionTypes.CREATED_NEW_MODULE_FOLDER : ActionTypes.CREATED_NEW_OWNER_FOLDER),
                    payload: {
                        value: parameters[type]
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createNewModule(payload, callback){
        return (dispatch) => {
            FolderService.createNewModule(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_NEW_MODULE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default folderActions;
