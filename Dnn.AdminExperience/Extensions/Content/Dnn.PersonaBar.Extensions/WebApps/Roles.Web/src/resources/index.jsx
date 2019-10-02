import util from "../utils";

const resx = {
    get(key) {
        let moduleName = util.moduleName;
        return util.utilities.getResx(moduleName, key);
    }
};
export default resx;