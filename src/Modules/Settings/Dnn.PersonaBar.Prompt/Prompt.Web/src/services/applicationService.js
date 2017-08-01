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

    getCommandList(callback) {
        const sf = this.getServiceFramework("Command");
        sf.get("List", {}, callback);
    }

    runCommand(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Command");
        sf.post("Cmd", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;