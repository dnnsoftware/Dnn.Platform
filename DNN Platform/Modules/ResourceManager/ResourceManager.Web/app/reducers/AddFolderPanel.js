import addFolderPanelActionsTypes from "../action types/addFolderPanelActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

const initialState = {
    expanded: false
};

export default function addFolderPanelReducer(state = initialState, action) {
    const data = action.data;
    switch (action.type) {
        case addFolderPanelActionsTypes.SHOW_ADD_FOLDER_PANEL:
            return { ...state, expanded: true };
        case addFolderPanelActionsTypes.FOLDER_CREATED:
            return { ...state, newFolderId: data.FolderID, expanded: false };
        case folderPanelActionsTypes.CLOSE_TOP_PANELS:
        case addFolderPanelActionsTypes.HIDE_ADD_FOLDER_PANEL: {
            let formData = { 
                name: "",
                folderType: state.defaultFolderType
            };
            let res = { ...state, formData, expanded: false, newFolderId: null };
            delete res.validationErrors;

            return res;
        }
        case addFolderPanelActionsTypes.FOLDER_MAPPINGS_LOADED: {
            let defaultFolderType = data && data[0] ? data[0].FolderMappingID : null;
            let formData = { ...state.formData, folderType: defaultFolderType };
            return { ...state, formData, folderMappings: data, defaultFolderType };
        }
        case addFolderPanelActionsTypes.CHANGE_NAME: {
            let formData = { ...state.formData, name: data };
            return { ...state, formData };
        }
        case addFolderPanelActionsTypes.CHANGE_FOLDER_TYPE: {
            let formData = { ...state.formData, folderType: data };
            return { ...state, formData };
        }
        case addFolderPanelActionsTypes.SET_VALIDATION_ERRORS:
            return { ...state, validationErrors: data };
    }
    
    return state;
}