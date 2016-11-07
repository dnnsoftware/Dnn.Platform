import {
    portal as ActionTypes
} from "../actionTypes";
import
{ CommonPortalService as PortalService }
    from "../services";
import utilities from "utils";

function errorCallback(message) {
    utilities.notifyError(message);
}
const portalActions = {
    deletePortal(portalId, index, callback) {
        return (dispatch) => {
            PortalService.deletePortal(portalId, data => {
                if (data.Success) {
                    dispatch({
                        type: ActionTypes.DELETED_PORTAL,
                        payload: {
                            index,
                            portalId
                        }
                    });
                    if (callback) {
                        callback(data);
                    }
                } else {
                    errorCallback(data.ErrorMessage);
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
            console.log(ActionTypes);
            PortalService.createPortal(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_PORTAL,
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
    },
    loadPortals(searchParameters, append, callback) {
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                if (append) {
                    dispatch({
                        type: ActionTypes.RETRIEVED_PORTALS_CONCAT,
                        payload: {
                            portals: data.Results,
                            totalCount: data.TotalResults
                        }
                    });
                } else {
                    dispatch({
                        type: ActionTypes.RETRIEVED_PORTALS,
                        payload: {
                            portals: data.Results,
                            totalCount: data.TotalResults
                        }
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
