import util from "../utils";

class ModuleDefinitionService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    addOrUpdateModuleDefinition(payload, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("AddOrUpdateModuleDefinition", payload, callback);
    }
    deleteModuleDefinition(definitionId, callback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("DeleteModuleDefinition?definitionId=" + definitionId, {}, callback);
    }
    getSourceFolders(callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFolders", {}, callback);
    }
    getSourceFiles(_root, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFiles?root=" + _root, {}, callback);
    }
    getControlIcons(controlPath, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("LoadIcons?controlPath=" + controlPath, {}, callback);
    }
    addOrUpdateModuleControl(payload, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("AddOrUpdateModuleControl", payload, callback);
    }
}
const moduleDefinitionService = new ModuleDefinitionService();
export default moduleDefinitionService;