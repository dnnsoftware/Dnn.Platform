const resx = {
    get(key) {
        const util = window.dnn.initPages();
        let moduleName = util.moduleName;
        return util.utility.getResx(moduleName, key);
    }	
};
export default resx;
