import {webTab as ActionTypes}  from "../constants/actionTypes";
import webTabService from "../services/webTabService";
import localization from "../localization";

const webTabActions = {
    loadWebServerInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_WEB_TAB               
            });        
            
            webTabService.getWebServerInfo().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_WEB_TAB,
                    payload: {
                        webServerInfo: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_WEB_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingWebTab")
                    }
                });
            });        
        };
    }
};

export default webTabActions;