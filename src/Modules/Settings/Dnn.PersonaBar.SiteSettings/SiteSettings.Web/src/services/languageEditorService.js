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
        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;
        return sf;
    }

    getRootResourcesFolder(mode, callback) {
        const sf = this.getServiceFramework("Languages");
        sf.get("GetRootResourcesFolders?mode=" + mode, {}, callback);
    }

    getSubRootResources(folder, callback) {
        const sf = this.getServiceFramework("Languages");
        sf.get("GetSubRootResources?currentFolder=" + folder, {}, callback);
    }

    getResxEntries(parameters, callback) {
        const sf = this.getServiceFramework("Languages");
        sf.get("GetResxEntries?" + serializeQueryStringParameters(parameters), {}, callback);
    }

    enableLocalizedContent(parameters, callback, failureCallback) {
        const sf = this.getServiceFramework("Languages");
        sf.post("EnableLocalizedContent?"  + serializeQueryStringParameters(parameters), {}, callback, failureCallback);
    }

    disableLocalizedContent(callback, failureCallback) {
        const sf = this.getServiceFramework("Languages");
        sf.post("DisableLocalizedContent", {}, callback, failureCallback);
    }

    getLocalizationProgress(callback) {
        const sf = this.getServiceFramework("Languages");
        sf.get("GetLocalizationProgress", {}, callback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;