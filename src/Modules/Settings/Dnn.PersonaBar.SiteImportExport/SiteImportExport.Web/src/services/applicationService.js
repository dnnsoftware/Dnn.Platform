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

    getPortals(callback, errorCallback) {
        const sf = this.getServiceFramework("PersonaBar", "Portals");
        sf.get("getPortals", {}, callback, errorCallback);
    }

    getInitialPortalTabs(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }

    getDescendantPortalTabs(parameters, parentIdObj, callback, errorCallback) {
        const params = Object.assign({}, parameters, parentIdObj);
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
        const serializedParams = serializeQueryStringParameters(params);
        sf.get(`GetTabsDescendants?${serializedParams}`, {}, callback, errorCallback);
    }

    getAllJobs(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.getsilence("AllJobs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }

    getLastJobTime(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("LastJobTime?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }

    getJobDetails(jobId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.getsilence("JobDetails?jobId=" + jobId, {}, callback, errorCallback);
    }

    exportSite(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Export", payload, callback, errorCallback);
    }

    importSite(payload, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Import", payload, callback, errorCallback);
    }

    getImportPackages(parameters, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("GetImportPackages?"+ serializeQueryStringParameters(parameters), {}, callback, errorCallback);
    }

    verifyImportPackage(packageId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("VerifyImportPackage?packageId=" + packageId, {}, callback, errorCallback);
    }

    cancelJob(jobId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("CancelProcess?jobId=" + jobId, {}, callback, errorCallback);
    }

    deleteJob(jobId, callback, errorCallback) {
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("RemoveJob?jobId=" + jobId, {}, callback, errorCallback);
    }

}
const applicationService = new ApplicationService();
export default applicationService;