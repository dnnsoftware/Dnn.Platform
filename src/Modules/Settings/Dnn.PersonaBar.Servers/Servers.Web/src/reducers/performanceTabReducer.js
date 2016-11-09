import {performanceTab as ActionTypes}  from "../constants/actionTypes";

export default function webTabReducer(state = {
    performanceSettings: {},
    pageStatePersistenceMode: "",
    errorMessage: "",
    loading: false,
    saving: false,
    incrementingVersion: false
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
                saving: true
            }; 
        case ActionTypes.SAVED_PERFORMANCE_SETTINGS: 
            return { ...state,
                saving: false
            };
        case ActionTypes.ERROR_SAVING_PERFORMANCE_SETTINGS:
            return { ...state,
                saving: false,
                errorMessage: action.payload.errorMessage
            };
        case ActionTypes.INCREMENT_VERSION:
            return { ...state,
                incrementingVersion: true
            }; 
        case ActionTypes.INCREMENTED_VERSION: 
            return { ...state,
                incrementingVersion: false
            };
        case ActionTypes.ERROR_INCREMENTING_VERSION:
            return { ...state,
                incrementingVersion: false,
                errorMessage: action.payload.errorMessage
            };
        default:
            return state;     
    }
}