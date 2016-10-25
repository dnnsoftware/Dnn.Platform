import serviceFramework from "./serviceFramework";

const getApplicationInfo = function () {
    return serviceFramework.get("SystemInfoApplication", "GetApplicationInfo");
};

const applicationTabService = {
    getApplicationInfo: getApplicationInfo
};

export default applicationTabService; 