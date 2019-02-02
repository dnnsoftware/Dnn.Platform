import {logsTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    logs: {},
    selectedLog: "",
    errorMessage: "",
    logData: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_LOGS_TAB:
            return { ...state,
                logs: {},
                selectedLog: "",
                errorMessage: "",
                logData: ""
            };
        case ActionTypes.LOADED_LOGS_TAB:
            return { ...state,
                logs: action.payload.logs,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_LOGS_TAB:
            return { ...state,
                logs: {},
                errorMessage:  action.payload.errorMessage
            }; 
        
        case ActionTypes.LOAD_LOG:
            return { ...state,
                selectedLog: action.payload.log,
                errorMessage: ""
            };
        case ActionTypes.LOADED_LOG:
            return { ...state,
                logData: action.payload.log,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_LOG:
            return { ...state,
                errorMessage:  action.payload.errorMessage
            }; 
        default:
            return state;     
    }
}