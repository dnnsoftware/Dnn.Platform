import infiniteScrollActionsTypes from "../action types/infiniteScrollActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

const initialState = {
    maxScrollTop: 0
};

export default function dialogModalReducer(state = initialState, action) {
    const data = action.data;
    
    switch (action.type) {
        case infiniteScrollActionsTypes.SET_MAX_SCROLL_TOP : {
            return { ...state, maxScrollTop: data};
        }
        case folderPanelActionsTypes.CONTENT_LOADED : 
        case folderPanelActionsTypes.FILES_SEARCHED : {
            return { ...state, maxScrollTop: 0 };
        }
    }
    
    return state;
}