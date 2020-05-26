import {webTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    webServerInfo: {},
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_WEB_TAB:
            return { ...state,
                webServerInfo: {},
                errorMessage: ""
            };
        case ActionTypes.LOADED_WEB_TAB:
            return { ...state,
                webServerInfo: action.payload.webServerInfo,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_WEB_TAB:
            return { ...state,
                webServerInfo: {},
                errorMessage:  action.payload.errorMessage
            };  
        default:
            return state;      
    }
}