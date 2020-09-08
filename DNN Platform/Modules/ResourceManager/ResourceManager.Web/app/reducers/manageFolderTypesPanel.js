import manageFolderTypesPanelActionTypes from "../action types/manageFolderTypesPanelActionsTypes";
import folderPanelActionTypes from "../action types/folderPanelActionsTypes";

const initialState = {
    expanded: false
};

export default function manageFolderTypesPanelReducer(state = initialState, action) {
    switch (action.type) {
        case manageFolderTypesPanelActionTypes.SHOW_MANAGE_FOLDER_TYPES_PANEL:
            return { ...state, expanded: true };
        case folderPanelActionTypes.CLOSE_TOP_PANELS:
        case manageFolderTypesPanelActionTypes.HIDE_MANAGE_FOLDER_TYPES_PANEL:
            return { ...state, expanded: false };
        case manageFolderTypesPanelActionTypes.ADD_FOLDER_TYPE_URL_LOADED:
            return { ...state, addFolderTypeUrl: action.data };
    }

    return state;
}