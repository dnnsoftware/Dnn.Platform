import utils from "../utils";
import serviceFramework from "./serviceFramework";

const getUpgradeSettings = function () {
  return serviceFramework.get("Upgrades", "GetSettings");
};

const listUpgrades = function () {
  return serviceFramework.get("Upgrades", "List");
};

const deletePackage = function (packageName) {
  return serviceFramework.post("Upgrades", "Delete", { packageName });
};

const startUpgrade = function (packageName) {
  return serviceFramework.post("Upgrades", "StartUpgrade", { packageName });
};

const uploadPackage = function (file, callback, errorCallback) {
  const sf = utils.getServiceFramework();
  sf.moduleRoot = "PersonaBar";
  sf.controller = "Upgrades";
  let formData = new FormData();
  formData.append("POSTFILE", file);
  sf.postfile("Upload", formData, callback, errorCallback);
};

const upgradeService = {
  getUpgradeSettings,
  listUpgrades,
  uploadPackage,
  deletePackage,
  startUpgrade,
};

export default upgradeService;
