import {databaseTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    databaseServerInfo: {},
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_DATABASE_TAB:
            return { ...state,
                databaseServerInfo: {},
                errorMessage: ""
            };
        case ActionTypes.LOADED_DATABASE_TAB:
            return { ...state,
                databaseServerInfo: action.payload.databaseServerInfo,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_DATABASE_TAB:
            return { ...state,
                databaseServerInfo: {},
                errorMessage:  action.payload.errorMessage
            }; 
        default:
            return state;     
    }
}