import { util } from "utils/helpers";

const Localization = {
    get(key) {
        let moduleName = util.moduleName;
        return util.utilities.getResx(moduleName, key);
    }
};
export default Localization;