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
        let sf = this.getServiceFramework("Languages");

        sf.get("GetRootResourcesFolders?mode=" + mode, {}, callback);
    }
    getSubRootResources(folder, callback) {
        let sf = this.getServiceFramework("Languages");

        sf.get("GetSubRootResources?currentFolder=" + folder, {}, callback);
    }
    getResxEntries(parameters, callback) {

        let sf = this.getServiceFramework("Languages");

        sf.get("GetResxEntries?" + serializeQueryStringParameters(parameters), {}, callback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;