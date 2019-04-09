import utilities from "../utils";

function mapPackageInformation(extensionBeingUpdated, addMode) {
    let _extensionBeingUpdated = {
        Name: extensionBeingUpdated.name.value,
        FriendlyName: extensionBeingUpdated.friendlyName.value,
        Description: extensionBeingUpdated.description.value,
        Version: extensionBeingUpdated.version.value,
        Owner: extensionBeingUpdated.owner.value,
        Url: extensionBeingUpdated.url.value,
        Organization: extensionBeingUpdated.organization.value,
        Email: extensionBeingUpdated.email.value,
        License: extensionBeingUpdated.releaseNotes && extensionBeingUpdated.license.value,
        ReleaseNotes: extensionBeingUpdated.releaseNotes && extensionBeingUpdated.releaseNotes.value
    };
    if (addMode) {
        delete _extensionBeingUpdated.License;
        delete _extensionBeingUpdated.ReleaseNotes;
        _extensionBeingUpdated.PackageType = extensionBeingUpdated.packageType.value;
    }
    return _extensionBeingUpdated;
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

        sf.moduleRoot = "PersonaBar";
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
    updateExtension(extensionBeingUpdated, editorActions, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: extensionBeingUpdated.packageId.value,
            portalId: utilities.settings.portalId,
            settings: mapPackageInformation(extensionBeingUpdated),
            editorActions: editorActions
        };
        sf.post("SavePackageSettings", payload, callback);
    }
    createNewExtension(extensionBeingUpdated, editorActions, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: -1,
            portalId: -1,
            settings: mapPackageInformation(extensionBeingUpdated, true),
            editorActions: editorActions
        };
        sf.post("CreateExtension", payload, callback, errorCallback);
    }
    downloadPackage(PackageType, FileName, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.get("DownloadPackage", payload, callback);
    }
    installAvailablePackage(PackageType, FileName, callback) {
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.post("InstallAvailablePackage", payload, callback);
    }
    deletePackage(payload, callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("DeletePackage", payload, callback);
    }
    parsePackage(file, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");

        let formData = new FormData();
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
    }
    deployAvailablePackage(cultureCode, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParseLanguagePackage?cultureCode=" + cultureCode, {}, callback, errorCallback);
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
    getDesktopModulePermissions(desktopModuleId, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetDesktopModulePermissions", { desktopModuleId: desktopModuleId }, callback, errorCallback);
    }
    saveDesktopModulePermissions(permissions, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("saveDesktopModulePermissions", { permissions: permissions }, callback, errorCallback);
    }
    parseAvailablePackage(FileName, PackageType, callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParsePackageFile", { FileName, PackageType }, callback, errorCallback);
    }
    getLocaleList(callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetLanguagesList", {}, callback, errorCallback);
    }
    getLocalePackagesList(callback, errorCallback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAllPackagesListExceptLangPacks", {}, callback, errorCallback);
    }
    getPackageUsageFilter(callback) {
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageUsageFilter", {}, callback);
    }
    getPackageUsage(portalId, packageId, callback) {
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            portalId: portalId,
            packageId: packageId
        };
        sf.get("GetPackageUsage?" + serializeQueryStringParameters(parameters), {}, callback);
    }
}
const extensionService = new ExtensionService();
export default extensionService;
