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

    getGeneralSettings(callback) {
        const sf = this.getServiceFramework("SEO");        
        sf.get("GetGeneralSettings", {}, callback);
    }    

    updateGeneralSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateGeneralSettings", payload, callback, failureCallback);
    }

    getRegexSettings(callback) {
        const sf = this.getServiceFramework("SEO");        
        sf.get("GetRegexSettings", {}, callback);
    }    

    updateRegexSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateRegexSettings", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;