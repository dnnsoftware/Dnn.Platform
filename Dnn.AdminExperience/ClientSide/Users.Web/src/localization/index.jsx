import utilities from "utils";
const resx = {
    get(key) {
        let moduleName = "Users";
        return utilities.getResx(moduleName, key);
    }
};
export default resx;