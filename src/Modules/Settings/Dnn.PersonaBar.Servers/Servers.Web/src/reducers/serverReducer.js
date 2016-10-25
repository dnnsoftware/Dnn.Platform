import {server as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    reloadPage: false,
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: false,
                errorMessage: ""
            };
        case ActionTypes.END_REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: true,
                errorMessage: ""
            };
        case ActionTypes.ERROR_REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: false,
                errorMessage:  action.payload.errorMessage
            };
        default:
            return { ...state
            };
    }
}
