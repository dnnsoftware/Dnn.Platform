const resx = {
    get(key) {
        const util = window.dnn.initServers();
        let moduleName = util.moduleName;
        return util.utility.getResx(moduleName, key);
    }
};

export default resx;