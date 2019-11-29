import utils from "./utils";

const localization = {
    get(key) {
        const moduleName = utils.getModuleName();
        return utils.getResx(moduleName, key);
    }	
};
export default localization;
