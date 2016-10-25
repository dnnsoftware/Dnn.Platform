import utilities from "../utils";

function mapPackageInformation(extensionBeingUpdated) {
    // return extensionBeingUpdated;
    return {
        Name: extensionBeingUpdated.name.value,
        FriendlyName: extensionBeingUpdated.friendlyName.value,
        Description: extensionBeingUpdated.description.value,
        Version: extensionBeingUpdated.version.value,
        Owner: extensionBeingUpdated.owner.value,
        Url: extensionBeingUpdated.url.value,
        Organization: extensionBeingUpdated.organization.value,
        Email: extensionBeingUpdated.email.value,
        License: extensionBeingUpdated.license.value,
        ReleaseNotes: extensionBeingUpdated.releaseNotes.value
    };
}

function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class PermissionService {
    getServiceFramework(controller) {
        let sf = utilities.utilities.sf;

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    getDesktopModulePermissions(desktopModuleId, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetDesktopModulePermissions", { desktopModuleId: desktopModuleId }, callback, errorCallback);
    }
    saveDesktopModulePermissions(permissions, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("SaveDesktopModulePermissions", { permissions: permissions }, callback, errorCallback);
    }
}
const permissionService = new PermissionService();
export default permissionService;