const resx = {
    get(key) {
        const util = window.dnn.initPages();
        const moduleName = util.moduleName;
        return util.utilities.getResx(moduleName, key);
    }	
};
export default resx;
