import dialogModalActionsTypes from "../action types/dialogModalActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

export default function dialogModalReducer(state = {}, action) {
    const data = action.data;
    
    switch (action.type) {
        case dialogModalActionsTypes.OPEN_DIALOG_MODAL : {
            return Object.assign({}, state, data);
        }
        case dialogModalActionsTypes.CLOSE_DIALOG_MODAL :
        case folderPanelActionsTypes.FOLDER_DELETED :
        case folderPanelActionsTypes.DELETE_FOLDER_ERROR : {
            let res = { ...state };
            delete res.dialogMessage;
            delete res.yesFunction;
            delete res.noFunction;
            return res;
        }
    }
    
    return state;
}