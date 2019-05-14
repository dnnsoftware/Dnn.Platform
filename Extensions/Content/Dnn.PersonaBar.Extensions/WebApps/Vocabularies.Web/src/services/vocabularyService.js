import util from "../utils";
//import "fetch-ie8";
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class VocabularyService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getVocabularyList(searchParameters, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");
        searchParameters = Object.assign({}, searchParameters, {
            scopeTypeId: searchParameters.scopeTypeId > 0 ? searchParameters.scopeTypeId : "*"
        });
        sf.get("GetVocabularies?" + serializeQueryStringParameters(searchParameters), {}, callback, failureCallback);
    }
    addVocabulary(vocabulary, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateVocabulary", vocabulary, callback, failureCallback);
    }
    updateVocabulary(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateVocabulary", payload, callback, failureCallback);
    }
    deleteVocabulary(vocabularyId, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");
        sf.post("DeleteVocabulary?vocabularyId=" + vocabularyId, {}, callback, failureCallback);
    }
}
const vocabularyService = new VocabularyService();
export default vocabularyService;