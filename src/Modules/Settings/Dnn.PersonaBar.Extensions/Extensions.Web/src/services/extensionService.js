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
class ExtensionService {
    getServiceFramework(controller) {
        let sf = utilities.utilities.sf;

        sf.moduleRoot = "PersonaBar/AdminHost";
        sf.controller = controller;

        return sf;
    }
    getInstalledPackages(type, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetInstalledPackages?packageType=" + type, {}, callback);
    }
    getAvailablePackages(type, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackages?packageType=" + type, {}, callback);
    }
    getPackageTypes(callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageTypes", {}, callback);
    }
    getAvailablePackageTypes(callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackageTypes", {}, callback);
    }
    updateExtension(extensionBeingUpdated, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: extensionBeingUpdated.packageId.value,
            portalId: utilities.settings.portalId,
            settings: mapPackageInformation(extensionBeingUpdated),
            editorActions: (extensionBeingUpdated.packageType.value === "Module" ? {
                category: extensionBeingUpdated.category.value,
                dependencies: extensionBeingUpdated.dependencies.value,
                permissions: extensionBeingUpdated.permissions.value,
                hostPermissions: extensionBeingUpdated.hostPermissions.value,
                shareable: extensionBeingUpdated.shareable.value,
                premiummodule: extensionBeingUpdated.premiumModule.value,
                assignPortal: extensionBeingUpdated.assignedPortals.value,
                unassignPortal: extensionBeingUpdated.unassignedPortals.value,
                foldername: extensionBeingUpdated.folderName.value,
                businesscontroller: extensionBeingUpdated.businessController.value
            } : {})
        };
        sf.post("SavePackageSettings", payload, callback);
    }
    downloadPackage(PackageType, FileName, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.post("DownloadPackage", payload);
    }
    installAvailablePackage(packageType, packageName, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageType,
            fileName: packageName
        };
        sf.post("InstallAvailablePackage?" + serializeQueryStringParameters(payload), {}, callback);
    }
    deletePackage(packageId, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("DeletePackage", { id: packageId }, callback);
    }
    parsePackage(file, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
    }
    installPackage(file, callback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);

        sf.postfile("InstallPackage", formData, callback);
    }
    createNewModule(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        sf.post("CreateModule", payload, callback, errorCallback);
    }
    getPackageSettings(packageId, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            siteId: utilities.settings.portalId,
            packageId
        };
        sf.get("GetPackageSettings?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }
    getModuleCategories(callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("getModuleCategories", {}, callback, errorCallback);
    }
}
const extensionService = new ExtensionService();
export default extensionService;