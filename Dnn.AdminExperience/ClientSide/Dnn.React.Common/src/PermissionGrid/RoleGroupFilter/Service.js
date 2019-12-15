
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

    getRoleGroups(callback, errorCallback) {
        sf.moduleRoot = "PersonaBar";
        sf.controller = "Components";

        sf.get("GetRoleGroups", {}, callback, errorCallback);
    }
}
export default Service;