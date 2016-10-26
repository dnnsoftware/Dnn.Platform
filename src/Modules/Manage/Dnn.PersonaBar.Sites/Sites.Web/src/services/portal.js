import utilities from "utils";
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class PortalService {
    getServiceFramework(controller) {
        let sf = utilities.sf;

        sf.moduleRoot = "PersonaBar/Host";
        sf.controller = controller;

        return sf;
    }

    getPortals(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortals?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
    getPortalTemplates(callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalTemplates", {}, callback, errorCallback);
    }
    createPortal(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("CreatePortal", payload, callback, errorCallback);
    }

    /*
    addVocabulary(vocabulary, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateVocabulary", vocabulary, callback);
    }
    updateVocabulary(payload, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateVocabulary", payload, callback);
    } */
}
const portalService = new PortalService();
export default portalService;