
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
let sf = null;
class Service {
    constructor(serviceFramework, moduleRoot, controller) {
        sf = serviceFramework;
        sf.moduleRoot = moduleRoot;
        sf.controller = controller;
    }
    getPortalTabs(portalTabsParameters, callback, errorCallback) {
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
    }
    getPortalTab(portalTabParameters, callback, errorCallback) {
        sf.get("GetPortalTab?" + serializeQueryStringParameters(portalTabParameters), {}, callback, errorCallback);
    }
    getTabsDescendants(portalTabsParameters, callback, errorCallback) {
        sf.get("GetTabsDescendants?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
    }
    searchPortalTabs(portalTabsSearchParameters, callback, errorCallback) {
        sf.get("SearchPortalTabs?" + serializeQueryStringParameters(portalTabsSearchParameters), {}, callback, errorCallback);
    }
}
export default Service;