import itemDetailsActionsTypes from "../action types/itemDetailsActionsTypes";

const initialState = {
    maxScrollTop: 0
};

export default function itemDetailsReducer(state = initialState, action) {
    const data = action.data;
    
    switch (action.type) {
        case itemDetailsActionsTypes.EDIT_ITEM: {
            return { ...state, itemEditing: data };
        }
        case itemDetailsActionsTypes.CHANGE_NAME: {
            let itemEditing = { ...state.itemEditing, fileName: data, folderName: data};
            return { ...state, itemEditing };
        }
        case itemDetailsActionsTypes.CHANGE_TITLE: {
            let itemEditing = { ...state.itemEditing, title: data };
            return { ...state, itemEditing };
        }
        case itemDetailsActionsTypes.CHANGE_DESCRIPTION: {
            let itemEditing = { ...state.itemEditing, description: data };
            return { ...state, itemEditing };
        }
        case itemDetailsActionsTypes.SET_VALIDATION_ERRORS: {
            return { ...state, validationErrors: data };
        }
        case itemDetailsActionsTypes.ITEM_SAVED :
        case itemDetailsActionsTypes.CANCEL_EDIT_ITEM: {
            let res = { ...state };
            delete res.itemEditing;
            return res;
        }
    }
    
    return state;
}