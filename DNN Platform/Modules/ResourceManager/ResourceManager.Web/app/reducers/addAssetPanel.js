import addAssetPanelActionsTypes from "../action types/addAssetPanelActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

const initialState = {
    expanded: false,
    progress: {},
    uploadedFiles: []
};

function mutateFileProgress(progress, fileProgress) {
    return { ...progress, [fileProgress.fileName]: fileProgress };
}

export default function addFolderPanelReducer(state = initialState, action) {
    const data = action.data;
    switch (action.type) {
        case addAssetPanelActionsTypes.SHOW_ADD_ASSET_PANEL:
            return { ...state, expanded: true };
        case folderPanelActionsTypes.CLOSE_TOP_PANELS:
        case addAssetPanelActionsTypes.HIDE_ADD_ASSET_PANEL: {
            return { ...state, expanded: false };
        }
        case addAssetPanelActionsTypes.RESET_PANEL:
            return { ...state, error: null, progress: {}, uploadedFiles: [] };
        case addAssetPanelActionsTypes.ASSET_ADDED: {
            let uploadedFiles = [ ...state.uploadedFiles, data ];
            let fileProgress = { ...state.progress[data.fileName], completed: true, path: data.path, fileIconUrl: data.fileIconUrl };
            return { ...state, uploadedFiles, progress: mutateFileProgress(state.progress, fileProgress) };
        }
        case addAssetPanelActionsTypes.ASSET_ADDED_ERROR: {
            const fileProgress = { ...state.progress[data.fileName], fileName: data.fileName, error: data.message};
            return { ...state, progress: mutateFileProgress(state.progress, fileProgress) };
        }
        case addAssetPanelActionsTypes.UPDATE_PROGRESS: 
            return { ...state, progress: mutateFileProgress(state.progress, data) };
        case addAssetPanelActionsTypes.FILE_ALREADY_EXIST: {
            const fileProgress = { ...state.progress[data.fileName], path: data.path, fileIconUrl: data.fileIconUrl, percent: 0, alreadyExists: true };
            return { ...state, progress: mutateFileProgress(state.progress, fileProgress) };
        }
        case addAssetPanelActionsTypes.STOP_UPLOAD: {
            const fileProgress = { ...state.progress[data], alreadyExists:false, stopped: true };
            return { ...state, progress: mutateFileProgress(state.progress, fileProgress) };
        }
    }
    
    return state;
}