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

        sf.moduleRoot = "PersonaBar/Admin";
        sf.controller = controller;

        return sf;
    }

    getPortalSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetPortalSettings?portalId=" + portalId, {}, callback);
    }    

    updatePortalSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePortalSettings", payload, callback, failureCallback);
    }

    getDefaultPagesSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetDefaultPagesSettings?portalId=" + portalId, {}, callback);
    }    

    updateDefaultPagesSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateDefaultPagesSettings", payload, callback, failureCallback);
    }

    getMessagingSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetMessagingSettings?portalId=" + portalId, {}, callback);
    }    

    updateMessagingSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateMessagingSettings", payload, callback, failureCallback);
    }

    getProfileSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetProfileSettings?portalId=" + portalId, {}, callback);
    }    

    updateProfileSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileSettings", payload, callback, failureCallback);
    }

    getProfileProperties(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetProfileProperties?portalId=" + portalId, {}, callback);
    }  

    getProfileProperty(propertyId, portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetProfileProperty?propertyId=" + propertyId + "&portalId=" + portalId, {}, callback);
    } 

    getProfilePropertyLocalization(propertyName, propertyCategory, cultureCode, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetProfilePropertyLocalization?propertyName=" + propertyName + "&propertyCategory=" + propertyCategory + "&cultureCode=" + cultureCode, {}, callback);
    }  

    updateProfileProperty(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileProperty", payload, callback, failureCallback);
    } 

    addProfileProperty(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddProfileProperty", payload, callback, failureCallback);
    } 

    deleteProfileProperty(propertyId, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteProfileProperty?propertyId=" + propertyId + "&portalId=", {}, callback, failureCallback);
    } 

    updateProfilePropertyLocalization(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfilePropertyLocalization", payload, callback, failureCallback);
    } 

    getUrlMappingSettings(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetUrlMappingSettings?portalId=" + portalId, {}, callback);
    }  

    getSiteAliases(portalId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetSiteAliases?portalId=" + portalId, {}, callback);
    } 

    updateUrlMappingSettings(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateUrlMappingSettings", payload, callback, failureCallback);
    }

    getSiteAlias(aliasId, callback) {
        const sf = this.getServiceFramework("SiteSettings");        
        sf.get("GetSiteAlias?portalAliasId=" + aliasId, {}, callback);
    } 

    addSiteAlias(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddSiteAlias", payload, callback, failureCallback);
    }

    updateSiteAlias(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateSiteAlias", payload, callback, failureCallback);
    }

    deleteSiteAlias(aliasId, callback, failureCallback) {
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteSiteAlias?portalAliasId=" + aliasId, {}, callback, failureCallback);
    } 
}
const applicationService = new ApplicationService();
export default applicationService;