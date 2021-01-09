import {serversTab as ActionTypes}  from "../constants/actionTypes";
import serversTabService from "../services/serversTabService";
import localization from "../localization";

const serversTabActions = {
    loadServers() {    
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SERVERS              
            });        
            
            serversTabService.getServers().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_SERVERS,
                    payload: {
                        servers: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_SERVERS,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingServers")
                    }
                });
            });        
        };
    }
};

export default serversTabActions;