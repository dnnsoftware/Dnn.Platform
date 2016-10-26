import {
    portal as ActionTypes
} from "constants/actionTypes";
import 
    PortalService
 from "../services/PortalService";
import utilities from "utils";

function errorCallback(message) {
    let utils = window.dnn.initSites().utility;
    utils.notify(message);
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
    deletePortal(portalId, index, callback) {
        return (dispatch) => {
            PortalService.deletePortal(portalId, data => {
                dispatch({
                    type: ActionTypes.DELETED_PORTAL_TEMPLATE,
                    payload: {
                        index,
                        portalId
                    }
                });
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
