import util from "../utils";
class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }

    getGeneralSettings(callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("GetGeneralSettings", {}, callback);
    }

    updateGeneralSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateGeneralSettings", payload, callback, failureCallback);
    }

    getRegexSettings(callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("GetRegexSettings", {}, callback);
    }

    updateRegexSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateRegexSettings", payload, callback, failureCallback);
    }

    testUrl(pageId, queryString, customPageName, callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrl?pageId=" + pageId + "&queryString=" + encodeURIComponent(queryString) + "&customPageName=" + encodeURIComponent(customPageName), {}, callback);
    }

    testUrlRewrite(uri, callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrlRewrite?uri=" + uri, {}, callback);
    }

    getSitemapSettings(callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapSettings", {}, callback);
    }

    updateSitemapSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapSettings", payload, callback, failureCallback);
    }

    getSitemapProviders(callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapProviders", {}, callback);
    }

    updateSitemapProvider(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapProvider", payload, callback, failureCallback);
    }

    createVerification(verification, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("CreateVerification?verification=" + verification, {}, callback, failureCallback);
    }

    clearCache(callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("ResetCache", {}, callback, failureCallback);
    }

    getExtensionUrlProviders(callback) {
        const sf = this.getServiceFramework("SEO");
        sf.get("GetExtensionUrlProviders", {}, callback);
    }

    updateExtensionUrlProviderStatus(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateExtensionUrlProviderStatus", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;