import {applicationTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    applicationInfo: {},
    isApplicationInfoLoaded: false,
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_APPLICATION_TAB:
            return { ...state,
                applicationInfo: {},
                isApplicationInfoLoaded: false,
                errorMessage: ""
            };
        case ActionTypes.LOADED_APPLICATION_TAB:
            return { ...state,
                applicationInfo: action.payload.applicationInfo,
                isApplicationInfoLoaded: true,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_APPLICATION_TAB:
            return { ...state,
                applicationInfo: {},
                isApplicationInfoLoaded: false,
                errorMessage:  action.payload.errorMessage
            };
        default:
            return state;
    }
}