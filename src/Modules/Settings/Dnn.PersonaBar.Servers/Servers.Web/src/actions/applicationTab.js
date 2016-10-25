import {applicationTab as ActionTypes}  from "../constants/actionTypes";
import applicationTabService from "../services/applicationTabService";
import localization from "../localization";

const applicationTabActions = {
    loadApplicationInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_APPLICATION_TAB               
            });        
            
            applicationTabService.getApplicationInfo().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_APPLICATION_TAB,
                    payload: {
                        applicationInfo: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_APPLICATION_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingApplicationTab")
                    }
                });
            });        
        };
    }
};

export default applicationTabActions;