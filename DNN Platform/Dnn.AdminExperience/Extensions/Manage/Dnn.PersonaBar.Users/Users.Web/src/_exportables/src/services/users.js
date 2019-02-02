import utilities from "utils";
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
        let sf = utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }
    getUsers(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
    getUserDetails(userDetailsParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserDetail?" + serializeQueryStringParameters(userDetailsParameters), {}, callback, errorCallback);
    }
    updateUserBasicInfo(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateUserBasicInfo", userDetails, callback, errorCallback);
    }
    getUserFilters(callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback, errorCallback);
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
    deleteUser(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("SoftDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    hardDeleteUser(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("HardDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    restoreUser(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("RestoreDeletedUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    updateSuperUserStatus(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateSuperUserStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    updateAuthorizeStatus(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateAuthorizeStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    unlockUser(userDetails, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("UnlockUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
    }
    //User Roles Methods
    getUserRoles(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
    getSuggestRoles(searchParameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("GetSuggestRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
    }
    saveUserRole(payload, notifyUser, isOwner, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("SaveUserRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, payload, callback, errorCallback);
    }
    removeUserRole(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.post("RemoveUserRole", payload, callback, errorCallback);
    }
    
    passwordStrengthOptions(callback, errorCallback) {
        const sf = this.getServiceFramework("Users");
        sf.get("PasswordStrengthOptions",{},callback,errorCallback);
    }
}
const userService = new UserService();
export default userService;