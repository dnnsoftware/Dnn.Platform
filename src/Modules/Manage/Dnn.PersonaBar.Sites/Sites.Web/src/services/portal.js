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
    getPortals(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortals?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
}
const portalService = new PortalService();
export default portalService;