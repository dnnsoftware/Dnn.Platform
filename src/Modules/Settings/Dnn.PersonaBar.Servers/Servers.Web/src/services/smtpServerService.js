import serviceFramework from "./serviceFramework";

const getSmtpSettings = function () {
    return serviceFramework.get("ServerSettingsSmtp", "GetSmtpSettings");
};

const smtpServerService = {
    getSmtpSettings: getSmtpSettings
};

export default smtpServerService; 