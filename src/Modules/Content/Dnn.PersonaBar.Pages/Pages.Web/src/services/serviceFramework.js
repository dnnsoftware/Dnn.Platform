import utils from "../utils";

function serializeQueryStringParameters(obj) {
    const s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

function getServiceFramework() {
    const sf = utils.getServiceFramework(); 

    sf.moduleRoot = "PersonaBar/Admin";
    sf.controller = window.dnn.pages.apiController;

    return sf;
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

const serviceFramework = {
    get(method, searchParameters) {
        const sf = getServiceFramework();
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(searchParameters), {}, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    },

    post(method, payload) {
        const sf = getServiceFramework();
        return new Promise((callback, errorCallback) => {
            sf.post(method, payload, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }
};

export default serviceFramework;