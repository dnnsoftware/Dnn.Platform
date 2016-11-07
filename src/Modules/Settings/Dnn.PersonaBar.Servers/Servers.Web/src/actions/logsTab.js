import {logsTab as ActionTypes}  from "../constants/actionTypes";
import logsTabService from "../services/logsTabService";
import localization from "../localization";

const logsTabActions = {
    loadLogsServerInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_LOGS_TAB               
            });        
            
            logsTabService.getLogs().then(logs => {
                dispatch({
                    type: ActionTypes.LOADED_LOGS_TAB,
                    payload: {
                        logs: logs
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_LOGS_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingLogsTab")
                    }
                });
            });        
        };
    },
       
    loadSelectedLog(log) {
        return (dispatch) => {
            if (!log) {
                return;
            }
            
            const logName = log.value;
            dispatch({
                type: ActionTypes.LOAD_LOG,
                payload: {
                    log: logName
                }               
            });        
            
            logsTabService.getLog(logName).then(response => {
                dispatch({
                    type: ActionTypes.LOADED_LOG,
                    payload: {
                        log: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_LOG,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingLog")
                    }
                });
            });        
        };
    }
};

export default logsTabActions;