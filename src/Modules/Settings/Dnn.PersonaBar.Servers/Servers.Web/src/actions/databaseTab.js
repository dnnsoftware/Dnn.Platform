import {databaseTab as ActionTypes}  from "../constants/actionTypes";
import databaseTabService from "../services/databaseTabService";
import localization from "../localization";

const databaseTabActions = {
    loadDatabaseServerInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_DATABASE_TAB               
            });        
            
            databaseTabService.getDataBaseServerInfo().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_DATABASE_TAB,
                    payload: {
                        databaseServerInfo: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_DATABASE_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingDatabaseTab")
                    }
                });
            });        
        };
    }
};

export default databaseTabActions;