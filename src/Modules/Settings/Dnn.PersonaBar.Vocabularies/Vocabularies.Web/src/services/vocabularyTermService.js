import util from "../utils";
class VocabularyTermService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getVocabularyTerms(vocabularyTermId, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.get("GetTermsByVocabularyId?vocabularyId=" + vocabularyTermId + "&pageIndex=0", {}, callback, failureCallback);
    }
    addVocabularyTerm(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("CreateTerm", payload, callback, failureCallback);
    }
    updateVocabularyTerm(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Vocabularies");

        sf.post("UpdateTerm", payload, callback, failureCallback);
    }
    deleteVocabularyTerm(termId, callback, failureCallback) {

        const sf = this.getServiceFramework("Vocabularies");

        sf.post("DeleteTerm?termId=" + termId, {}, callback, failureCallback);
    }
}
const vocabularyTermService = new VocabularyTermService();
export default vocabularyTermService;