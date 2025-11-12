import { ChunkToUpload } from "models/ChunkToUpload";
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

const uploadPackage = function (chunk: ChunkToUpload, callback, errorCallback) {
  const sf = utils.getServiceFramework();
  sf.moduleRoot = "PersonaBar";
  sf.controller = "Upgrades";
  let formData = new FormData();
  formData.append("chunk", chunk.chunk);
  formData.append("start", chunk.start.toString());
  formData.append("totalSize", chunk.totalSize.toString());
  formData.append("fileId", chunk.fileId);
  sf.postfile("Upload", formData, callback, errorCallback);
};

const completeUpload = function (fileId: string, fileName: string) {
  return serviceFramework.post("Upgrades", "UploadComplete", { fileId, fileName });
};

const upgradeService = {
  getUpgradeSettings,
  listUpgrades,
  uploadPackage,
  completeUpload,
  deletePackage,
  startUpgrade,
};

export default upgradeService;
