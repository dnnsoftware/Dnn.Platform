import { smtpServerTab as ActionTypes } from "../constants/actionTypes";

export default function smtpServerTabReducer(state = {
    smtpServerInfo: {},
    errorMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_SMTP_SERVER_TAB:
            return { ...state,
                smtpServerInfo: {},
                errorMessage: ""
            };
        case ActionTypes.LOADED_SMTP_SERVER_TAB:
            return { ...state,
                smtpServerInfo: action.payload.smtpServerInfo,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_SMTP_SERVER_TAB:
            return { ...state,
                smtpServerInfo: {},
                errorMessage: action.payload.errorMessage
            };
        case ActionTypes.CHANGE_SMTP_SERVER_MODE:
            return { ...state,
                smtpServerInfo: {
                    ...state.smtpServerInfo,
                    smtpServerMode: action.payload.smtpServeMode
                }
            };
        case ActionTypes.CHANGE_SMTP_AUTHENTICATION: {
            if (state.smtpServerInfo.smtpServerMode === "h") {
                return { ...state,
                    smtpServerInfo: {...state.smtpServerInfo,
                        host: {...state.smtpServerInfo.host,
                            smtpAuthentication: action.payload.smtpAuthentication
                        }
                    }
                };
            }

            return { ...state,
                smtpServerInfo: {...state.smtpServerInfo,
                    site: {...state.smtpServerInfo.site,
                        smtpAuthentication: action.payload.smtpAuthentication
                    }
                }
            };
        }
        default:
            return state;
    }
}