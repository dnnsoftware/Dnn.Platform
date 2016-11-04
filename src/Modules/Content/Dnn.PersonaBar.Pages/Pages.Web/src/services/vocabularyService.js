import utils from "../utils";
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
class VocabularyService {
    getServiceFramework(controller) {
        const sf = utils.getServiceFramework(); 

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    getVocabularyList(searchParameters, callback) {
        const sf = this.getServiceFramework("Vocabularies");
        searchParameters = Object.assign({}, searchParameters, {
            scopeTypeId: searchParameters.scopeTypeId > 0 ? searchParameters.scopeTypeId : "*" 
        });
        sf.get("GetVocabularies?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }
    getVocabularyTerms(vocabularyTermId, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.get("GetTermsByVocabularyId?vocabularyId=" + vocabularyTermId + "&pageIndex=0&pageSize=10000", {}, callback);
    }
    addVocabularyTerm(payload, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateTerm", payload, callback);
    }
    addVocabulary(vocabulary, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateVocabulary", vocabulary, callback);
    }
    updateVocabulary(payload, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateVocabulary", payload, callback);
    }
    updateVocabularyTerm(payload, callback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateTerm", payload, callback);
    }
}
const vocabularyService = new VocabularyService();
export default vocabularyService;