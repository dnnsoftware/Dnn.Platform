import util from "../utils";

function errorCallback(message) {
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
}

class ModuleDefinitionService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    addOrUpdateModuleDefinition(payload, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("AddOrUpdateModuleDefinition", payload, callback, errorCallback);
    }
    deleteModuleDefinition(definitionId, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("DeleteModuleDefinition?definitionId=" + definitionId, {}, callback, errorCallback);
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
    addOrUpdateModuleControl(payload, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("AddOrUpdateModuleControl", payload, callback, errorCallback);
    }
    deleteModuleControl(controlId, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("DeleteModuleControl?controlId=" + controlId, {}, callback, errorCallback);
    }
}
const moduleDefinitionService = new ModuleDefinitionService();
export default moduleDefinitionService;