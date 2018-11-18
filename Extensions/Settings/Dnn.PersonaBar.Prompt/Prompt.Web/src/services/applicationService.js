import { util } from "utils/helpers";

class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;
        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;
        return sf;
    }
    getServiceFrameworkWithRoot(moduleRoot, controller) {
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
        sf.controller = controller;
        return sf;
    }

    runCommand(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Command");
        sf.post("Cmd", payload, callback, failureCallback);
    }
    changeUserMode(payload, callback, failureCallback) {
        const sf = this.getServiceFrameworkWithRoot("InternalServices", "ControlBar");
        sf.post("ToggleUserMode", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;