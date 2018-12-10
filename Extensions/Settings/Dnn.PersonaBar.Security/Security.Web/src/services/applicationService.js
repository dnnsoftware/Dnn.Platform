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
    getServiceFramework(controller) {
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }

    getIpFilters(callback) {
        const sf = this.getServiceFramework("Security");        
        sf.get("GetIpFilters", {}, callback);
    }
    
    getIpFilter(searchParameters, callback) {
        const sf = this.getServiceFramework("Security");        
        searchParameters = Object.assign({}, searchParameters, {
            
        });
        sf.get("GetIpFilter?" + serializeQueryStringParameters(searchParameters), {}, callback);
    } 

    deleteIpFilter(filterId, callback, failureCallback) {
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteIpFilter?filterId=" + filterId, {}, callback, failureCallback);
    }    

    getBasicLoginSettings(cultureCode, callback) {
        const sf = this.getServiceFramework("Security");        
        sf.get("GetBasicLoginSettings?cultureCode=" + cultureCode, {}, callback);
    }

    updateBasicLoginSettings(payload, callback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateBasicLoginSettings", payload, callback);
    }

    updateIpFilter(payload, callback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateIpFilter", payload, callback);
    }

    getMemberSettings(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetMemberSettings", {}, callback);
    }

    updateMemberSettings(payload, callback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateMemberSettings", payload, callback);
    }

    getRegistrationSettings(callback) {
        const sf = this.getServiceFramework("Security");        
        sf.get("GetRegistrationSettings", {}, callback);
    }

    updateRegistrationSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateRegistrationSettings", payload, callback, failureCallback);
    }

    getSslSettings(callback) {
        const sf = this.getServiceFramework("Security");        
        sf.get("GetSslSettings", {}, callback);
    }

    updateSslSettings(payload, callback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateSslSettings", payload, callback);
    }

    getOtherSettings(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetOtherSettings", {}, callback);
    }

    updateOtherSettings(payload, callback) {
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateOtherSettings", payload, callback);
    }

    getSecurityBulletins(callback, failureCallback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetSecurityBulletins", {}, callback, failureCallback);
    }

    getSuperuserActivities(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetSuperuserActivities", {}, callback);
    }

    getAuditCheckResults(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResults", {}, callback);
    }

    getAuditCheckResult(id, callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResult", {id: id}, callback);
    }

    searchFileSystemAndDatabase(searchParameters, callback) {
        const sf = this.getServiceFramework("Security");        
        searchParameters = Object.assign({}, searchParameters, {
            
        });

        if (this.searchRequest && this.searchRequest.readyState !== 4) {
            if (window.dnn) {
                window.dnn.loading = false;
            }
            this.searchRequest.abort();
        }

        this.searchRequest = sf.get("SearchFileSystemAndDatabase?" + serializeQueryStringParameters(searchParameters), {}, callback);
    } 

    getLastModifiedSettings(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedSettings", {}, callback);
    }

    getLastModifiedFiles(callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedFiles", {}, callback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;