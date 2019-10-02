import util from "../utils";
function errorCallback(message) {
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
}

class CreatePackageService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getPackageManifest(packageId, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.get("GetPackageManifest?packageId=" + packageId, {}, callback, errorCallback);
    }
    createManifest(payload, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("CreateManifest", payload, callback, errorCallback);
    }
    generateManifestPreview(payload, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("CreateNewManifest", payload, callback, errorCallback);
    }
    createPackage(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("CreatePackage", payload, callback, errorCallback);
    }
    refreshPackageFiles(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("RefreshPackageFiles", payload, callback, errorCallback);
    }
}
const createPackageService = new CreatePackageService();
export default createPackageService;