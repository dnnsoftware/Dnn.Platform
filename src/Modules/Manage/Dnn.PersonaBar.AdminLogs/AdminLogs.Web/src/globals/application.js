import utilities from "../utils";
const adminLogs = {
    init() {
        // This setting is required and define the public path 
        // to allow the web application to download assets on demand 
        // eslint-disable-next-line no-undef
        // __webpack_public_path__ = options.publicPath;
        let options = window.dnn.initAdminLogs();
        
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
        utilities.settings = options.settings;

        // delay the styles loading after the __webpack_public_path__ is set
        // this allows the fonts associated to be loaded properly in production
        require("../less/style.less");
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default adminLogs;