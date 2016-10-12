import util from "../utils";

class InstallationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    parsePackage(file, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
    }
    installPackage(file, callback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);


        sf.postfile("InstallPackage", formData, callback);
    }
}
const installationService = new InstallationService();
export default installationService;