import serviceFramework from "./serviceFramework";

const getLogs = function () {
    return serviceFramework.get("ServerSettingsLogs", "GetLogs")
        .then(response => {
            return response.Results.LogList.map((log) => {
                return {
                    value: log,
                    label: log
                };
            });
        }
    );
};

const getLog = function (fileName) {
    return serviceFramework.get("ServerSettingsLogs", "GetLogFile", {fileName: fileName});
};

const databaseTabService = {
    getLogs: getLogs,
    getLog: getLog
};

export default databaseTabService; 