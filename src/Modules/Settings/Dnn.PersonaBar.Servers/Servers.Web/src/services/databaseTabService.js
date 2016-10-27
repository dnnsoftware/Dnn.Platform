import serviceFramework from "./serviceFramework";

const getDataBaseServerInfo = function () {
    return serviceFramework.get("SystemInfoDatabase", "GetDatabaseServerInfo");
};

const databaseTabService = {
    getDataBaseServerInfo: getDataBaseServerInfo
};

export default databaseTabService; 