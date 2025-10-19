import serviceFramework from "./serviceFramework";

const listUpgrades = function () {
  return serviceFramework.get("Upgrades", "List");
};

const upgradeService = {
  listUpgrades,
};

export default upgradeService;
