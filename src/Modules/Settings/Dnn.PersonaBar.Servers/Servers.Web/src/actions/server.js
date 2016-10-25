import {server as ActionTypes}  from "../constants/actionTypes";
import serverService from "../services/serverService";
import localization from "../localization";

const serverActions = {
    restartApplication() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.REQUEST_RESTART_APPLICATION               
            });        
            
            serverService.restartApplication().then(response => {
                dispatch({
                    type: ActionTypes.END_REQUEST_RESTART_APPLICATION,
                    payload: {
                        url: response.url
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_REQUEST_RESTART_APPLICATION,
                    payload: {
                        errorMessage: localization.get("errorMessageRestartingApplication")
                    }
                });
            });        
        };
    },
    clearCache() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.REQUEST_CLEAR_CACHE               
            });        
            
            serverService.clearCache().then(response => {
                dispatch({
                    type: ActionTypes.END_REQUEST_CLEAR_CACHE,
                    payload: {
                        url: response.url
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_REQUEST_CLEAR_CACHE,
                    payload: {
                        errorMessage: localization.get("errorMessageClearingCache")
                    }
                });
            });        
        };
    }
};

export default serverActions;