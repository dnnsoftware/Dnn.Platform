import {portal as ActionTypes}  from "constants/actionTypes";
import PortalService from "services/PortalService";
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
                    callback();
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
    },
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
    getPortalTabs(portalTabsParameters, callback) {
        return (dispatch) => {
            PortalService.getPortalTabs(portalTabsParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_TABS,
                    payload: {
                        portalTabs: [data.Results]
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getTabsDescendants(portalTabsParameters, callback) {
        return () => {
            PortalService.getTabsDescendants(portalTabsParameters, data => {
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
    }
};

export default portalActions;
