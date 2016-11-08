import {performanceTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    performanceSettings: {},
    pageStatePersistenceMode: "",
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: {},
                errorMessage: ""
            };
        case ActionTypes.LOADED_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: action.payload.performanceSettings,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: {},
                errorMessage:  action.payload.errorMessage
            }; 
        default:
            return state;     
    }
}