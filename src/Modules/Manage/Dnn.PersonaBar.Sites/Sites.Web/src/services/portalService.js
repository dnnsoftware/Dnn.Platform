
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
        let sf = window.dnn.initSites().utility.sf;

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
    getPortalLocales(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalLocales?portalId=" + portalId, {}, callback, errorCallback);
    }
    deletePortal(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("DeletePortal?portalId=" + portalId, {}, callback, errorCallback);
    }

    getPortalTabs(portalTabsParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
    }
    getTabsDescendants(portalTabsParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.get("GetTabsDescendants?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
    }
    exportPortal(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Sites");
        sf.post("ExportPortalTemplate", payload, callback, errorCallback);
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