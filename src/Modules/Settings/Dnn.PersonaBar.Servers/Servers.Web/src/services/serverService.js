import serviceFramework from "./serviceFramework";

const restartApplication = function () {
    return serviceFramework.post("Server", "RestartApplication");
};
const clearCache = function () {
    return serviceFramework.post("Server", "ClearCache");
};

const getServersCount = function () {
    return serviceFramework.get("Server", "GetServersCount");
};

const serverService = {
    restartApplication,
    clearCache,
    getServersCount
};

export default serverService; 