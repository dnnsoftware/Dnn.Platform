import util from "../utils";
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class UserService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar/Admin";
        sf.controller = controller;

        return sf;
    }
    getUsers(searchParameters, callback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }
    getUserDetails(userDetailsParameters, callback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserDetail?" + serializeQueryStringParameters(userDetailsParameters), {}, callback);
    }
    updateAccountSettings(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateAccountSettings", userDetails, callback, errorCallback);
    }
    getUserFilters(callback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback);
    }
    createUser(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("CreateUser", userDetails, callback, errorCallback);
    }
    changePassword(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("ChangePassword", payload, callback, errorCallback);
    }
    forceChangePassword(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("ForceChangePassword?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    sendPasswordResetLink(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("SendPasswordResetLink?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }

}
const userService = new UserService();
export default userService;