import Api from "./api";

function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

const languageService = {
    getLanguages(tabId, callback) {
        const api = new Api("Pages");
        api.get("GetTabLocalization", {tabId}, callback);
    }
};

export default languageService;