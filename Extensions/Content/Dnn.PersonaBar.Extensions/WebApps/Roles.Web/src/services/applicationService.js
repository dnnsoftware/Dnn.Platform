import util from "../utils";
class ApplicationService {
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }

    getRoleGroups(reload, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleGroups", { reload: reload }, callback, errorCallback);
    }

    getRoles(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoles", parameters, callback, errorCallback);
    }

    saveRoleGroup(group, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRoleGroup", group, callback, errorCallback);
    }

    deleteRoleGroup(group, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRoleGroup", group, callback, errorCallback);
    }

    saveRole(assignExistUsers, role, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRole?assignExistUsers=" + assignExistUsers, role, callback, errorCallback);
    }

    deleteRole(role, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRole", role, callback, errorCallback);
    }

    getSuggestUsers(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetSuggestUsers", parameters, callback, errorCallback);
    }

    getRoleUsers(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleUsers", parameters, callback, errorCallback);
    }

    addUserToRole(parameters, notifyUser, isOwner, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("AddUserToRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, parameters, callback, errorCallback);
    }

    removeUserFromRole(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("Roles");
        sf.post("RemoveUserFromRole", parameters, callback, errorCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;