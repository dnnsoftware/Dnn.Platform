import util from "./utils";

const resx = {
    get(key) {
        let moduleName = util.getModuleName();
        return util.getResx(moduleName, key);
    }
};

export default resx;