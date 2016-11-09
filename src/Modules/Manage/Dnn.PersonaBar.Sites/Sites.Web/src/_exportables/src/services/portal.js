import utilities from "utils";
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class PortalService {
    getServiceFramework(controller) {
        let sf = utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getPortalLocales(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalLocales?portalId=" + portalId, {}, callback, errorCallback);
    }
    deletePortal(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("DeletePortal?portalId=" + portalId, {}, callback, errorCallback);
    }
    exportPortal(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("ExportPortalTemplate", payload, callback, errorCallback);
    }
    getPortalTemplates(callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalTemplates", {}, callback, errorCallback);
    }
    createPortal(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("CreatePortal", payload, callback, errorCallback);
    }
    getPortals(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortals?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
}
const portalService = new PortalService();
export default portalService;