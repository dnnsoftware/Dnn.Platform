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
        sf.get("GetTabLocalization?" + serializeQueryStringParameters({ tabId }), {}, callback);
    }

    makePageTranslatable(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageTranslatable?" + serializeQueryStringParameters({ tabId }), {}, callback);
    }

    makePageNeutral(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageNeutral?" + serializeQueryStringParameters({ tabId }), {}, callback);
    }

    addMissingLanguages(tabId, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("AddMissingLanguages?" + serializeQueryStringParameters({ tabId }), {}, callback);
    }

    notifyTranslators(params, callback) {
        const sf = this.getServiceFramework("Pages");
        sf.post("NotifyTranslators", params, callback);
    }
}

const languageService = new LanguageService();
export default languageService;