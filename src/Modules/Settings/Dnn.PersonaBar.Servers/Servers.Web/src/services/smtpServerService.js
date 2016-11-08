import serviceFramework from "./serviceFramework";
import utils from "../utils";

function getControllerName() {
    return utils.isHostUser() ? "ServerSettingsSmtpHost" : "ServerSettingsSmtpAdmin";
}

const getSmtpSettings = function () {    
    return serviceFramework.get(getControllerName(), "GetSmtpSettings");
};

const updateSmtpSettings = function (parameters) {    
    return serviceFramework.post(getControllerName(), "UpdateSmtpSettings", parameters);
};

const smtpServerService = {
    getSmtpSettings: getSmtpSettings,
    updateSmtpSettings: updateSmtpSettings
};

export default smtpServerService; 