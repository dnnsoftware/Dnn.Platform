import util from "utils";

const Localization = {
  get(key) {
    let moduleName = "SiteGroups";
    return util.getResx(moduleName, key);
  },
};
export default Localization;
