import util from "../utils";
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar/Admin";
        sf.controller = controller;

        return sf;
    }

    getPortalSettings(callback) {
        const sf = this.getServiceFramework("SiteInfo");        
        sf.get("GetPortalSettings", {}, callback);
    }    

    updatePortalSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteInfo");
        sf.post("UpdatePortalSettings", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;