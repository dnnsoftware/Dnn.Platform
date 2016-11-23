import util from "../utils";

class InstallationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    parsePackage(file, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
    }
    installPackage(file, legacyType, callback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);

        if (legacyType) {
            sf.postfile("InstallPackage?legacySkin=" + legacyType, formData, callback, util.utilities.notifyError);
        } else {
            sf.postfile("InstallPackage", formData, callback, util.utilities.notifyError);
        }
    }
}
const installationService = new InstallationService();
export default installationService;