import serviceFramework from "./serviceFramework";

const getServers = function () {
  return serviceFramework.get("SystemInfoServers", "GetServers");
};

const deleteServer = function (serverId) {
  return serviceFramework.post("SystemInfoServers", "DeleteServer", {
    serverId: serverId,
  });
};

const editServerUrl = function (serverId, newUrl) {
  return serviceFramework.post("SystemInfoServers", "EditServerUrl", {
    serverId: serverId,
    newUrl: newUrl,
  });
};

const deleteNonActiveServers = function () {
  return serviceFramework.post("SystemInfoServers", "DeleteNonActiveServers");
};

const serversTabService = {
  getServers: getServers,
  deleteServer: deleteServer,
  editServerUrl: editServerUrl,
  deleteNonActiveServers: deleteNonActiveServers,
};

export default serversTabService;
