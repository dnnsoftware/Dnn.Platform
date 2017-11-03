import utils from "../utils";

function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

class LanguageService {
    getServiceFramework(controller) {
        let sf = utils.getServiceFramework();
        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;
        return sf;
    }

    getLanguages(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.get("GetTabLocalization?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
    }

    makePageTranslatable(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageTranslatable?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
    }

    makePageNeutral(tabId, callback, failureCallback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageNeutral?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback, failureCallback);
    }

    addMissingLanguages(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("AddMissingLanguages?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
    }

    notifyTranslators(params, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("NotifyTranslators", params, callback);
    }

    updateTabLocalization(params, callback, failureCallback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("UpdateTabLocalization", params, callback, failureCallback);
    }
    
    restoreModule(params, callback, failureCallback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("RestoreModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
    }

    getContentLocalizationEnabled( callback) {
        const sf = this.getServiceFramework("Pages");
        sf.get("GetContentLocalizationEnabled", {}, callback);
    }

    deleteModule(params, callback, failureCallback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("DeleteModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
    }
}

const languageService = new LanguageService();
export default languageService;