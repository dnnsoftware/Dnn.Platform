import globalActionsTypes from "../action types/globalActionsTypes";

const initialState = {
    isEditMode: false,
    moduleId: -1,
    moduleName: "",
    tabId: -1,
    portalId: -1
};

export default function moduleReducer(state = initialState, action) {
    const data = action.data;

    switch (action.type) {
        case globalActionsTypes.MODULE_PARAMETERS_LOADED:
            return Object.assign({}, state, data);    
    }
    return state;
}