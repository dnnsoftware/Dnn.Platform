import utilities from "utils";
const resx = {
    get(key) {
        let moduleName = "Sites";
        return utilities.getResx(moduleName, key);
    }
};
export default resx;