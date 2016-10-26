import {
    portal as ActionTypes
} from "constants/actionTypes";
import {
    PortalService
} from "services";
import utilities from "utils";

function errorCallback(message) {
    let utils = window.dnn.initSites().utility;
    utils.notify(message);
}
const portalActions = {
    loadPortals(searchParameters, callback) {
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    payload: {
                        portals: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getPortalTemplates(callback) {
        return (dispatch) => {
            PortalService.getPortalTemplates(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_TEMPLATES,
                    payload: {
                        templates: data.Results.Templates,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createPortal(payload, callback) {
        return (dispatch) => {
            PortalService.createPortal(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_PORTAL_TEMPLATE,
                    payload: {
                        Portal: data.Portal,
                        Success: data.Success,
                        ErrorMessage: data.ErrorMessage
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
