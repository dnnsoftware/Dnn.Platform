import actionTypes from "../action types/addFolderPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";
import ItemsService from "../services/itemsService";

const addFolderPanelActions = {
    showPanel() {
        return (dispatch) => {
            dispatch ({
                type: folderPanelActionTypes.CLOSE_TOP_PANELS
            });
            
            dispatch ({
                type: actionTypes.SHOW_ADD_FOLDER_PANEL
            });
        };
    },
    hidePanel() {
        return (dispatch) => {
            dispatch ({
                type: actionTypes.HIDE_ADD_FOLDER_PANEL
            });
        };
    },
    loadFolderMappings() {
        return (dispatch) => {
            ItemsService.loadFolderMappings().then((response) => {
                dispatch({
                    type: actionTypes.FOLDER_MAPPINGS_LOADED,
                    data: response
                });
            }).catch(() => {
                dispatch({
                    type: actionTypes.LOAD_FOLDER_MAPPINGS_ERROR
                });
            });
        };
    },
    changeName(event)
    {
        return {
            type: actionTypes.CHANGE_NAME,
            data: event.target.value
        };
    },
    changeFolderType(folderType)
    {
        return {
            type: actionTypes.CHANGE_FOLDER_TYPE,
            data: folderType
        };
    },
    addFolder(data) {
        const {formData, folderPanelState} = data;
        const { numItems, sorting } = folderPanelState;
        const folderId = folderPanelState.folder ? folderPanelState.folder.folderId : folderPanelState.homeFolderId;

        let newFolderData = {
            folderName: formData.name,
            FolderMappingId: formData.folderType,
            ParentFolderId: folderId
        };

        return (dispatch) => {
            ItemsService.addFolder(newFolderData)
            .then(
                addFolderResponse => 
                ItemsService.getContent(folderId, 0, numItems, sorting)
                .then(
                    getContentResponse => {
                        dispatch({
                            type: folderPanelActionTypes.CONTENT_LOADED,
                            data: getContentResponse
                        });

                        dispatch({
                            type: actionTypes.FOLDER_CREATED,
                            data: addFolderResponse
                        });
                    }
                ),
                reason => dispatch({
                    type: actionTypes.ADD_FOLDER_ERROR,
                    data: reason.data ? reason.data.message : null
                })
            )
            .catch(
                dispatch({
                    type: actionTypes.ADD_FOLDER_ERROR
                })
            );
        };
    },
    setValidationErrors(validationErrors) {
        return {
            type: actionTypes.SET_VALIDATION_ERRORS,
            data: validationErrors
        };
    }
};

export default addFolderPanelActions;
