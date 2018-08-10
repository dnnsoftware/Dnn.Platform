import {server as ActionTypes}  from "../constants/actionTypes";
import localization from "../localization";

export default function webTabReducer(state = {
    reloadPage: false,
    infoMessage: "",
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: false,
                infoMessage: "",
                errorMessage: ""
            };
        case ActionTypes.END_REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: true,
                infoMessage: localization.get("infoMessageRestartingApplication"),
                errorMessage: ""
            };
        case ActionTypes.ERROR_REQUEST_RESTART_APPLICATION:
            return { ...state,
                reloadPage: false,
                infoMessage: "",
                errorMessage:  action.payload.errorMessage
            };
        case ActionTypes.REQUEST_CLEAR_CACHE:
            return { ...state,
                reloadPage: false,
                infoMessage: "",
                errorMessage: ""
            };
        case ActionTypes.END_REQUEST_CLEAR_CACHE:
            return { ...state,
                reloadPage: true,
                infoMessage: localization.get("infoMessageClearingCache"),
                errorMessage: ""
            };
        case ActionTypes.ERROR_REQUEST_CLEAR_CACHE:
            return { ...state,
                reloadPage: false,
                infoMessage: "",
                errorMessage:  action.payload.errorMessage
            };

        default:
            return state;
    }
}
