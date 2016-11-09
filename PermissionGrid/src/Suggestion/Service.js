
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
let sf = null;
class Service {
    constructor(serviceFramework) {
        sf = serviceFramework;
    }

    getSuggestions(actionName, parameters, callback, errorCallback) {
        sf.moduleRoot = "PersonaBar";
        sf.controller = "Components";

        sf.get(actionName, parameters, callback, errorCallback);
    }
}
export default Service;