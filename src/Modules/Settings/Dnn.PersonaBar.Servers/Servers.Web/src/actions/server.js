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
    },
    getServersInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SERVER_INFO               
            });        
            
            serverService.getServersCount().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_SERVER_INFO,
                    payload: {
                        serversCount: response.serversCount
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_SERVER_INFO,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingServersInfo")
                    }
                });
            });        
        };
    }
};

export default serverActions;