import actionTypes from "../action types/manageFolderTypesPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";

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
    }
};

export default manageFolderTypesPanelActions;