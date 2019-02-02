import {smtpServerTab as ActionTypes}  from "../constants/actionTypes";
import smtpServerService from "../services/smtpServerService";
import localization from "../localization";

const smtpServeTabActions = {
    loadSmtpServerInfo() {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SMTP_SERVER_TAB               
            });        
            
            smtpServerService.getSmtpSettings().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_SMTP_SERVER_TAB,
                    payload: {
                        smtpServerInfo: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_SMTP_SERVER_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingSmtpServerTab")
                    }
                });
            });        
        };
    },
    changeSmtpServerMode(smtpServeMode) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_SMTP_SERVER_MODE,
                payload: {
                    smtpServeMode
                }
            });
        };
    },
    changeSmtpAuthentication(smtpAuthentication) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_SMTP_AUTHENTICATION,
                payload: {
                    smtpAuthentication
                }
            });
        };
    },
    changeSmtpConfigurationValue(key, value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_SMTP_CONFIGURATION_VALUE,
                payload: { 
                    field: key,
                    value
                }
            });  
        };
    },
    updateSmtpServerSettings(parameters) {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATE_SMTP_SERVER_SETTINGS               
            });        
            
            smtpServerService.updateSmtpSettings(parameters).then(response => {
                dispatch({
                    type: ActionTypes.UPDATED_SMTP_SERVER_SETTINGS,
                    payload: {
                        success: response.success
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_UPDATING_SMTP_SERVER_SETTINGS,
                    payload: {
                        errorMessage: localization.get("errorMessageUpdatingSmtpServerTab")
                    }
                });
            });        
        };
    },
    sendTestEmail(parameters) {       
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEND_TEST_EMAIL               
            });        
            
            smtpServerService.sendTestEmail(parameters).then(response => {
                dispatch({
                    type: ActionTypes.SENT_TEST_EMAIL,
                    payload: {
                        success: response.success,
                        infoMessage: response.confirmationMessage,
                        errorMessage: response.errMessage
                    }
                });  
            }).catch((data) => {
                let response = JSON.parse(data.responseText);
                dispatch({
                    type: ActionTypes.ERROR_SENDING_TEST_EMAIL,
                    payload: {
                        errorMessage: response.errMessage
                    }
                });
            });        
        };
    }
};

export default smtpServeTabActions;