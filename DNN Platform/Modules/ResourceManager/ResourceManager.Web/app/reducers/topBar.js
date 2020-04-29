import topBarActionsTypes from "../action types/topBarActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

export default function topBarReducer(state = {}, action) {
    const data = action.data;
    
    switch (action.type) {
        case topBarActionsTypes.CHANGE_SEARCH_FIELD: {
            return { ...state, search: data };
        }
        case folderPanelActionsTypes.CONTENT_LOADED: {
            return { ...state, search: undefined };
        }
    }
    
    return state;
}