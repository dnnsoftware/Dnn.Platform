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

    getLogList(searchParameters, callback) {
        const sf = this.getServiceFramework("AdminLogs");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetLogItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }

    getPortalList(addAll, callback) {
        const sf = this.getServiceFramework("Portals");
        sf.get("GetPortals?addAll=" + addAll, {}, callback);
    }

    /*Get all the types*/
    getLogTypes(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogTypes", {}, callback);
    }

    deleteLogItems(payload, callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogItems", payload, callback);
    }

    emailLogItems(payload, callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("EmailLogItems", payload, callback);
    }

    clearLog(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("ClearLog", {}, callback);
    }

    /*Get  Keep Most Recent Options*/
    getKeepMostRecentOptions(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetKeepMostRecentOptions", {}, callback);
    }

    /*Get  Occurence Options*/
    getOccurrenceOptions(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetOccurrenceOptions", {}, callback);
    }

    /*Get a particular log setting details*/
    getLogSettingById(parameters, callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSetting?" + serializeQueryStringParameters(parameters), {}, callback);
    }

    /*Get latest log setting details*/
    getLatestLogSetting(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLatestLogSetting", {}, callback);
    }
    getLogSettings(callback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSettings", {}, callback);
    }

    /*Update a log type setting*/
    updateLogSetting(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("UpdateLogSetting", payload, callback, failureCallback);
    }

    /*Add a log type setting*/
    addLogSetting(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("AddLogSetting", payload, callback, failureCallback);
    }

    deleteLogSetting(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogSetting", payload, callback, failureCallback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;