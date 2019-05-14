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

    getTaskStatusList(searchParameters, callback) {
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.getsilence("GetScheduleStatus?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }

    startSchedule(callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StartSchedule", {}, callback);
    }

    stopSchedule(callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StopSchedule", {}, callback);
    }

    getSchedulerSettings(callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.get("GetSchedulerSettings", {}, callback);
    }

    updateSchedulerSettings(payload, callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateSchedulerSettings", payload, callback);
    }

    getScheduleItemHistory(searchParameters, callback) {
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItemHistory?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }

    getScheduleItems(searchParameters, callback) {
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }

    getServerList(callback) {
        const sf = this.getServiceFramework("TaskScheduler");
        sf.get("GetServers", {}, callback);
    }

    getGetScheduleItem(searchParameters, callback) {
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItem?" + serializeQueryStringParameters(searchParameters), {}, callback);
    }

    deleteScheduleItem(payload, callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("DeleteSchedule", payload, callback);
    }

    createScheduleItem(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("CreateScheduleItem", payload, callback, failureCallback);
    }

    updateScheduleItem(payload, callback, failureCallback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateScheduleItem", payload, callback, failureCallback);
    }

    runScheduleItem(payload, callback) {
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("RunSchedule", payload, callback);
    }
}
const applicationService = new ApplicationService();
export default applicationService;