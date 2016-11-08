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
    changePerformanceSettingsMode(mode) {
        
    },
    changeCacheSettingMode(mode) {
        
    },
    incrementVersion() {
        
    },
    save() {
        
    }
};

export default performanceTabActions;