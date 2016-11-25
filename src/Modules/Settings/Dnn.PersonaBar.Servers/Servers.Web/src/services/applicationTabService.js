import serviceFramework from "./serviceFramework";
import utils from "../utils";

function getControllerName() {
    return utils.isHostUser() ? "SystemInfoApplicationHost" : "SystemInfoApplicationAdmin";
}

const getApplicationInfo = function () {
    return serviceFramework.get(getControllerName(), "GetApplicationInfo");
};

const applicationTabService = {
    getApplicationInfo: getApplicationInfo
};

export default applicationTabService; 