import messageModalActionsTypes from "../action types/messageModalActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";
import itemDetailsActionsTypes from "../action types/itemDetailsActionsTypes";
import addFolderPanelActionsTypes from "../action types/addFolderPanelActionsTypes";
import localizeService from "../services/localizeService";

export default function messageModalReducer(state = {}, action) {
    const data = action.data;

    switch (action.type) {
        case messageModalActionsTypes.CLOSE_MESSAGE_MODAL : {
            let res = { ...state };
            delete res.infoMessage;
            delete res.errorMessage;
            return res;
        }
        case itemDetailsActionsTypes.EDIT_ITEM_ERROR :
        case itemDetailsActionsTypes.SAVE_ITEM_ERROR :
        case folderPanelActionsTypes.DELETE_FILE_ERROR:
        case folderPanelActionsTypes.DELETE_FOLDER_ERROR:
        case folderPanelActionsTypes.LOAD_CONTENT_ERROR : 
        case addFolderPanelActionsTypes.ADD_FOLDER_ERROR: {
            return { ...state, infoMessage: data};
        }
        case itemDetailsActionsTypes.ITEM_SAVED : {
            return { ...state, infoMessage: localizeService.getString("ItemSavedMessage") };
        }
        case folderPanelActionsTypes.URL_COPIED_TO_CLIPBOARD : {
            return { ...state, infoMessage: localizeService.getString("UrlCopiedMessage") };
        }
    }
    
    return state;
}