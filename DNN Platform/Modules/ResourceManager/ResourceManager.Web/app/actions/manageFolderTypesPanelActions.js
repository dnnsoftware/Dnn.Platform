import actionTypes from "../action types/manageFolderTypesPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";
import addFolderActionTypes from "../action types/addFolderPanelActionsTypes";
import ItemsService from "../services/itemsService";

const manageFolderTypesPanelActions = {
    showPanel() {
        return dispatch => {
            dispatch ({
                type: folderPanelActionTypes.CLOSE_TOP_PANELS
            });

            dispatch ({
                type: actionTypes.SHOW_MANAGE_FOLDER_TYPES_PANEL
            });
        }
    },
    hidePanel() {
        return dispatch => {
            dispatch ({
                type: actionTypes.HIDE_MANAGE_FOLDER_TYPES_PANEL
            });
        };
    },
    removeFolderType(folderMappingId) {
        return (dispatch) => {
            ItemsService.removeFolderType(folderMappingId)
            .then(
                ItemsService.loadFolderMappings()
                .then(response => {
                    dispatch({
                        type: addFolderActionTypes.FOLDER_MAPPINGS_LOADED,
                        date: response
                    });
                })
                .catch(() => {
                    dispatch({
                        type: addFolderActionTypes.LOAD_FOLDER_MAPPINGS_ERROR,
                    })
                })
            )
        }
    },
};

export default manageFolderTypesPanelActions;