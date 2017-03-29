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

class ApplicationService {
    getServiceFramework(moduleRoot, controller) {
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
        sf.controller = controller;
        return sf;
    }

    getPortalLocales(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("PersonaBar", "SiteImportExport");
        sf.get("GetPortalLocales?portalId=" + portalId, {}, callback, errorCallback);
    }

    getPortals(callback, errorCallback) {
        const sf = this.getServiceFramework("PersonaBar", "Portals");
        sf.get("getPortals", {}, callback, errorCallback);
    }

    getAllJobs(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("AllJobs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }

    getLastExportDate(portalId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("LastExportUtcTime?portalId=" + portalId, {}, callback, errorCallback);
    }

    getJobDetails(jobId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("JobDetails?jobId=" + jobId, {}, callback, errorCallback);
    }

    exportSite(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Export", payload, callback, errorCallback);
    }

    importSite(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Import", payload, callback, errorCallback);
    }

    getImportPackages(callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("GetImportPackages", {}, callback, errorCallback);
    }

    verifyImportPackage(packageId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("VerifyImportPackage?packageId=" + packageId, {}, callback, errorCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;