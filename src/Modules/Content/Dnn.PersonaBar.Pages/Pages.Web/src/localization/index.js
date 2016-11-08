const resx = {
    get(key) {
        const util = window.dnn.initPages();
        const moduleName = util.moduleName;
        return util.getResx(moduleName, key);
    }	
};
export default resx;
