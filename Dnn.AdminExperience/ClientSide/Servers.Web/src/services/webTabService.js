import serviceFramework from "./serviceFramework";

const getWebServerInfo = function () {
    return serviceFramework.get("SystemInfoWeb", "GetWebServerInfo");
};

const webTabService = {
    getWebServerInfo: getWebServerInfo
};

export default webTabService; 