import {performanceTab as ActionTypes}  from "../constants/actionTypes";
import performanceTabService from "../services/performanceTabService";
import localization from "../localization";

const performanceTabActions = {
    loadPerformanceSettings() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_PERFORMANCE_TAB               
            });        
            
            performanceTabService.getPerformanceSettings().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_PERFORMANCE_TAB,
                    payload: {
                        performanceSettings: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PERFORMANCE_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingPerformanceTab")
                    }
                });
            });        
        };
    },
    changePerformanceSettingsValue(key, value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_PERFORMANCE_SETTINGS_VALUE,
                payload: { 
                    field: key,
                    value
                }
            });  
        };
    },
    incrementVersion(version, isGlobalSettings) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.INCREMENT_VERSION               
            });        
            
            const key = isGlobalSettings ? "currentHostVersion" : "currentPortalVersion" ;
            performanceTabService.incrementVersion(version, isGlobalSettings).then(response => {
                dispatch({
                    type: ActionTypes.INCREMENTED_VERSION,
                    payload: {
                        success: response.success
                    }
                });  
                dispatch({
                    type: ActionTypes.CHANGE_PERFORMANCE_SETTINGS_VALUE,
                    payload: { 
                        field: key,
                        value: parseInt(version, 10) + 1
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_INCREMENTING_VERSION,
                    payload: {
                        errorMessage: localization.get("errorMessageIncrementingVersion")
                    }
                });
            });        
        };
    },
    save(performanceSettings) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SAVE_PERFORMANCE_SETTINGS               
            });        
            
            performanceTabService.save(performanceSettings).then(response => {
                dispatch({
                    type: ActionTypes.SAVED_PERFORMANCE_SETTINGS,
                    payload: {
                        success: response.success
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PERFORMANCE_SETTINGS,
                    payload: {
                        errorMessage: localization.get("errorMessageSavingPerformanceSettingsTab")
                    }
                });
            });        
        };
    }
};

export default performanceTabActions;