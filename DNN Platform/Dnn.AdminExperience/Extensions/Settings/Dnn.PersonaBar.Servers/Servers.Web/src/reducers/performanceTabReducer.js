import {performanceTab as ActionTypes}  from "../constants/actionTypes";
import localization from "../localization";

export default function webTabReducer(state = {
    performanceSettings: {},
    pageStatePersistenceMode: "",
    errorMessage: "",
    loading: false,
    saving: false,
    incrementingVersion: false,
    infoMessage: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: {},
                loading: true,
                errorMessage: ""
            };
        case ActionTypes.LOADED_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: action.payload.performanceSettings,
                loading: false,
                errorMessage: ""
            };
        case ActionTypes.ERROR_LOADING_PERFORMANCE_TAB:
            return { ...state,
                performanceSettings: {},
                loading: false,
                errorMessage:  action.payload.errorMessage
            }; 
        case ActionTypes.CHANGE_PERFORMANCE_SETTINGS_VALUE: {
            const field = action.payload.field;
            const value = action.payload.value;
            const performanceSettings = {
                ...state.performanceSettings
            };
        
            performanceSettings[field] = value;
            return { ...state, performanceSettings};
        }
        case ActionTypes.SAVE_PERFORMANCE_SETTINGS:
            return { ...state,
                saving: true,
                errorMessage: "",
                infoMessage: ""
            }; 
        case ActionTypes.SAVED_PERFORMANCE_SETTINGS: 
            return { ...state,
                saving: false,
                errorMessage: "",
                infoMessage: localization.get("SaveConfirmationMessage")
            };
        case ActionTypes.ERROR_SAVING_PERFORMANCE_SETTINGS:
            return { ...state,
                saving: false,
                errorMessage: action.payload.errorMessage,
                infoMessage: ""
            };
        case ActionTypes.INCREMENT_VERSION:
            return { ...state,
                incrementingVersion: true,
                infoMessage: ""
            }; 
        case ActionTypes.INCREMENTED_VERSION: 
            return { ...state,
                incrementingVersion: false,
                infoMessage: localization.get("VersionIncrementedConfirmation")
            };
        case ActionTypes.ERROR_INCREMENTING_VERSION:
            return { ...state,
                incrementingVersion: false,
                errorMessage: action.payload.errorMessage,
                infoMessage: ""
            };
        default:
            return state;     
    }
}