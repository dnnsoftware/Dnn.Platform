import actionTypes from "../action types/manageFolderTypesPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";
import addFolderPanelActions from "../actions/addFolderPanelActions";
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
                () => {
                    dispatch(addFolderPanelActions.loadFolderMappings());
                }
            );
        }
    },
    getAddFolderTypeUrl() {
        return dispatch => {
            ItemsService.getAddFolderTypeUrl()
            .then(data => {
                dispatch({
                    type: actionTypes.ADD_FOLDER_TYPE_URL_LOADED,
                    data
                });
            });
        }
    }
};

export default manageFolderTypesPanelActions;