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
class FolderService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getOwnerFolders(callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.get("GetOwnerFolders", {}, callback, errorCallback);
    }
    getModuleFolders(ownerFolder, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.get("GetModuleFolders?ownerFolder=" + ownerFolder, {}, callback, errorCallback);
    }
    getModuleFiles(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.get("GetModuleFiles?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }
    createNewFolder(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("CreateFolder?" + serializeQueryStringParameters(payload), {}, callback, errorCallback);
    }
}
const folderService = new FolderService();
export default folderService;