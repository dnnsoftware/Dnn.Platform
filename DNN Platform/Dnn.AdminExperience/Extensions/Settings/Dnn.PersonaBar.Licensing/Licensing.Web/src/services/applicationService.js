import util from "../utils";
class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
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