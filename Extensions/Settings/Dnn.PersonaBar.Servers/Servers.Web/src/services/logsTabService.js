import serviceFramework from "./serviceFramework";

const getLogs = function () {
    return serviceFramework.get("ServerSettingsLogs", "GetLogs")
        .then(response => {
            const logList = response.Results.LogList.map((log) => {
                return {
                    value: log,
                    label: log
                };
            });
            
            const upgradeLogList = response.Results.UpgradeLogList.map((log) => {
                return {
                    value: log,
                    label: log,
                    upgradeLog: true
                };
            });
            
            return logList.concat(upgradeLogList);
        });
};

const getLog = function (fileName, upgradeLog) {
    if (upgradeLog) {
        return serviceFramework.get("ServerSettingsLogs", "GetUpgradeLogFile", {logName: fileName});    
    }
    
    return serviceFramework.get("ServerSettingsLogs", "GetLogFile", {fileName: fileName});
};

const databaseTabService = {
    getLogs: getLogs,
    getLog: getLog
};

export default databaseTabService; 