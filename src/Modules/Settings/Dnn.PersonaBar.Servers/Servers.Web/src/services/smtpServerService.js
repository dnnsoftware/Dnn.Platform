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

const sendTestEmail = function (parameters) {    
    return serviceFramework.post(getControllerName(), "SendTestEmail", parameters);
};

const smtpServerService = {
    getSmtpSettings,
    updateSmtpSettings,
    sendTestEmail
};

export default smtpServerService; 