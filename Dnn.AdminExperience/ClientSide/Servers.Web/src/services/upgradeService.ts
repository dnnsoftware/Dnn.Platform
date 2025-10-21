import utils from "../utils";
import serviceFramework from "./serviceFramework";

const listUpgrades = function () {
  return serviceFramework.get("Upgrades", "List");
};

const deletePackage = function (packageName) {
  return serviceFramework.post("Upgrades", "Delete", { packageName });
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
  listUpgrades,
  uploadPackage,
  deletePackage,
};

export default upgradeService;
