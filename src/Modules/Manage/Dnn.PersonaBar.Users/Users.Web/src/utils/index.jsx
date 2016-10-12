const utils = {
    init(options) {
        if (!options) {
            throw new Error("Options is undefined.");
        }
        console.log(options);
        this.utilities = options.utility;
        this.moduleName = options.moduleName;      
    },
    utilities: null,
    moduleName: ""
};
export default utils;