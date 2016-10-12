import util from "../utils";
import "fetch-ie8";
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

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }

    //Sample calls
    /* getVocabularyList(searchParameters, callback) {
        const sf = this.getServiceFramework("Vocabularies");
        sf.get("GetVocabularies?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }
    addVocabulary(vocabulary, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateVocabulary", vocabulary, callback);
    }
    updateVocabulary(payload, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateVocabulary", payload, callback);
    } */
}
const applicationService = new ApplicationService();
export default ApplicationService;