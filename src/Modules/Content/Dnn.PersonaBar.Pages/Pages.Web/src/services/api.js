import utils from "../utils";
import Promise from "es6-promise";

function serializeQueryStringParameters(obj) {
    const s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

function getErrorMessageFromXHRError(error) {
    if (error && 
        error.responseJSON && 
        error.responseJSON.Message) {
        return {
            message: error.responseJSON.Message
        };
    }
    return {
        message: null
    };
}

class Api {

    constructor(controller) {
        this.controller = controller;
        this.moduleRoot = "PersonaBar";
    }

    getServiceFramework() {
        const sf = utils.getServiceFramework(); 
        sf.moduleRoot = this.moduleRoot;
        sf.controller = this.controller;
        return sf;
    }

    get(method, searchParameters) {
        const sf = this.getServiceFramework();
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(searchParameters), {}, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }

    post(method, payload) {
        const sf = this.getServiceFramework();
        return new Promise((callback, errorCallback) => {
            sf.post(method, payload, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }
}
export default Api;