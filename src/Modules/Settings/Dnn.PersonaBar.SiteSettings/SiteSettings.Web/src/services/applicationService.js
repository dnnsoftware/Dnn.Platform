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

    getPortalSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetPortalSettings?portalId=" + portalId, {}, callback);
    }    

    updatePortalSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePortalSettings", payload, callback, failureCallback);
    }

    getDefaultPagesSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetDefaultPagesSettings?portalId=" + portalId, {}, callback);
    }    

    updateDefaultPagesSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateDefaultPagesSettings", payload, callback, failureCallback);
    }

    getMessagingSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetMessagingSettings?portalId=" + portalId, {}, callback);
    }    

    updateMessagingSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateMessagingSettings", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;