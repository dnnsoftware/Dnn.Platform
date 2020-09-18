import moment from "moment";
import serviceFramework from "./serviceFramework";

const getLogs = function () {
    return serviceFramework
        .get("ServerSettingsLogs", "GetLogs")
        .then(response => {
            const logList = response.Results.LogList.map(
                ({ Name, LastWriteTimeUtc, Size }) => {
                    return {
                        name: Name,
                        lastWriteTimeUtc: LastWriteTimeUtc,
                        size: Size,
                        upgradeLog: false
                    };
                });
            
            const upgradeLogList = response.Results.UpgradeLogList.map(
                ({ Name, LastWriteTimeUtc, Size }) => {
                    return {
                        name: Name,
                        lastWriteTimeUtc: LastWriteTimeUtc,
                        size: Size,
                        upgradeLog: true
                    };
                });
            
            return logList
                .concat(upgradeLogList)
                .sort((a, b) => moment(b.lastWriteTimeUtc) - moment(a.lastWriteTimeUtc));
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