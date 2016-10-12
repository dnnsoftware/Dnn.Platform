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

        sf.moduleRoot = "PersonaBar/Host";
        sf.controller = controller;

        return sf;
    }    

    getProduct(callback) {
        const sf = this.getServiceFramework("Licensing");
        sf.get("GetProduct", {}, callback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;