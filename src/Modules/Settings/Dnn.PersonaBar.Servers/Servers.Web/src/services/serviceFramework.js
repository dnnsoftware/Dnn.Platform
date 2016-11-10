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

function getServiceFramework(controllerName) {
    const sf = utils.getServiceFramework();

    sf.moduleRoot = "PersonaBar";
    sf.controller = controllerName;

    return sf;
}

const serviceFramework = {
    get(controllerName, method, parameters) {
        const sf = getServiceFramework(controllerName);
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
        });
    },
    post(controllerName, method, parameters) {
        const sf = getServiceFramework(controllerName);
        return new Promise((callback, errorCallback) => {
            sf.post(method, parameters, callback, errorCallback);
        });
    }
};

export default serviceFramework; 