const utils = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities.utility;
        this.moduleName = utilities.moduleName;
        this.settings = utilities.settings;
        this.siteRoot = utilities.siteRoot;    
    },
    utilities: null,
    moduleName: "",
    settings: {}
};
export default utils;