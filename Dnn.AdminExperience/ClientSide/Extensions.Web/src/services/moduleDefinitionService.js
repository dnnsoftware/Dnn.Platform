import util from "../utils";

function errorCallback(message) {
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
}

class ModuleDefinitionService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getSourceFolders(callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFolders", {}, callback, errorCallback);
    }
    getSourceFiles(_root, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFiles?root=" + _root, {}, callback, errorCallback);
    }
    getControlIcons(controlPath, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("LoadIcons?controlPath=" + controlPath, {}, callback, errorCallback);
    }
}
const moduleDefinitionService = new ModuleDefinitionService();
export default moduleDefinitionService;