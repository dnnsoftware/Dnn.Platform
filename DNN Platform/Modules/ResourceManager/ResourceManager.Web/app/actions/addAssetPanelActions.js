import actionTypes from "../action types/addAssetPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";
import ItemsService from "../services/itemsService";
import LocalizeService from "../services/localizeService";

let files = {};

function fileUploadedHandler(dispatch, file, overwrite, response) {
    const {alreadyExists, fileName, message} = response;

    if (alreadyExists) {
        files[fileName] = file;

        return dispatch ({
            type: actionTypes.FILE_ALREADY_EXIST,
            data: response
        });
    }

    if (!fileName || message) {
        return dispatch (fileUploadError(file.name, message));
    }
    
    files[fileName] = undefined;

    dispatch ({
        type: actionTypes.ASSET_ADDED,
        data: response
    });
}

function fileUploadError(fileName, message) {
    return {
        type: actionTypes.ASSET_ADDED_ERROR,
        data: {
            fileName,
            message
        }
    };
}

const addAssetPanelActions = {
    showPanel() {
        return (dispatch) => {
            dispatch ({
                type: folderPanelActionTypes.CLOSE_TOP_PANELS
            });

            dispatch ({
                type: actionTypes.SHOW_ADD_ASSET_PANEL
            });
        };
    },
    hidePanel() {
        return (dispatch) => {
            dispatch ({
                type: actionTypes.HIDE_ADD_ASSET_PANEL
            });

            dispatch ({
                type: actionTypes.RESET_PANEL
            });
        };
    },
    overwriteFile(fileName, folderPath, folderPanelState, trackProgress) {
        let file = files[fileName];
        return this.uploadFiles([file], folderPath, folderPanelState, trackProgress, true);
    },
    uploadFiles(files, folderPath, folderPanelState, trackProgress, overwrite=false) {
        const { numItems, sorting } = folderPanelState;
        const folderId = folderPanelState.folder ? folderPanelState.folder.folderId : folderPanelState.homeFolderId;

        return dispatch => {
            const uploadFilePromises = [];

            files.forEach (
                file => uploadFilePromises.push(ItemsService.uploadFile(file, folderPath, overwrite, trackProgress.bind(null, file.name))
                    .then(
                        response => fileUploadedHandler(dispatch, file, overwrite, response),
                        reason => dispatch (fileUploadError(file.name, reason.message))
                    ))
            );

            Promise.all(uploadFilePromises).then(
                () => ItemsService.getContent(folderId, 0, numItems, sorting)
            )
                .then(
                    getContentResponse => dispatch ({
                        type: folderPanelActionTypes.CONTENT_LOADED,
                        data: getContentResponse
                    })
                );
        };
    },
    trackProgress(fileName, progress) {
        return dispatch => 
            dispatch ({
                type: actionTypes.UPDATE_PROGRESS,
                data: {
                    fileName,
                    percent: progress.percent
                }
            });
    },
    stopUpload(fileName) {
        files[fileName] = undefined;

        return dispatch => dispatch ({
            type: actionTypes.STOP_UPLOAD,
            data: fileName
        });
    },
    fileSizeError(fileName, maxSize) {
        const fileTooBigMessage = LocalizeService.getString("FileSizeErrorMessage");
        return dispatch =>
            dispatch(fileUploadError(fileName, fileName + fileTooBigMessage + maxSize));
    },
    invalidExtensionError(fileName) {
        const invalidExtensionMessage = LocalizeService.getString("InvalidExtensionMessage");
        return dispatch =>
            dispatch(fileUploadError(fileName, fileName + invalidExtensionMessage));
    }
/*,
    startUpload() {
        return dispatch => 
            dispatch ({
                type: actionTypes.RESET_PANEL
            });
    },
    */
};

export default addAssetPanelActions;
