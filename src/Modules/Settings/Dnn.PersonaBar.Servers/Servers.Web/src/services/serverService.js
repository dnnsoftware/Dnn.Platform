import serviceFramework from "./serviceFramework";

const restartApplication = function () {
    return serviceFramework.post("Server", "RestartApplication");
};
const clearCache = function () {
    return serviceFramework.post("Server", "ClearCache");
};

const serverService = {
    restartApplication: restartApplication,
    clearCache: clearCache
};

export default serverService; 