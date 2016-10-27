import {
    portal as ActionTypes
} from "../actionTypes";
import 
    {CommonPortalService as PortalService}
 from "../services";
import utilities from "utils";

function errorCallback(message) {
    utilities.notify(message);
}
const portalActions = {
    getPortalLocales(portalId, callback) {
        return (dispatch) => {
            PortalService.getPortalLocales(portalId, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    exportPortal(payload, callback) {
        return (dispatch) => {
            PortalService.exportPortal(payload, data => {
                dispatch({
                    type: ActionTypes.EXPORTED_PORTAL_TEMPLATE,
                    payload: {
                        Success: data.Success,
                        Message: data.Message,
                        Template: data.Template
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    setPortalBeingExported(portalBeingExported, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_PORTAL_BEING_EXPORTED,
                payload: portalBeingExported
            });
            utilities.throttleExecution(callback);
        };
    }
};

export default portalActions;
