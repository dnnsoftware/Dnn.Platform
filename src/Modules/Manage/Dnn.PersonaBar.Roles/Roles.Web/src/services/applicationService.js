import util from "../utils";
import resx from "../resources";

function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

function showError(xhr) {
    let response = eval("(" + xhr.responseText + ")");
    let message = "Failed...";
    if (response["Error"]) {
        message = resx.get(response["Error"]);
    }

    util.utilities.notify(message);
}
class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }

    getRoleGroups(reload, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleGroups", { reload: reload }, callback, showError);
    }

    getRoles(parameters, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoles", parameters, callback);
    }

    saveRoleGroup(group, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRoleGroup", group, callback, showError);
    }

    deleteRoleGroup(group, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRoleGroup", group, callback, showError);
    }

    saveRole(assignExistUsers, role, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRole?assignExistUsers=" + assignExistUsers, role, callback, showError);
    }

    deleteRole(role, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRole", role, callback, showError);
    }

    getSuggestUsers(parameters, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetSuggestUsers", parameters, callback, showError);
    }

    getRoleUsers(parameters, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleUsers", parameters, callback, showError);
    }

    addUserToRole(parameters, notifyUser, isOwner, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("AddUserToRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, parameters, callback, showError);
    }

    removeUserFromRole(parameters, callback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("RemoveUserFromRole", parameters, callback, showError);
    }
}
const applicationService = new ApplicationService();
export default applicationService;