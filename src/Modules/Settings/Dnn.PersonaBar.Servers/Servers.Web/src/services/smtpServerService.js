import serviceFramework from "./serviceFramework";
import utils from "../utils";

function getControllerName() {
    return utils.isHostUser() ? "ServerSettingsSmtpHost" : "ServerSettingsSmtpAdmin";
}

const getSmtpSettings = function () {    
    return serviceFramework.get(getControllerName(), "GetSmtpSettings");
};

const smtpServerService = {
    getSmtpSettings: getSmtpSettings
};

export default smtpServerService; 