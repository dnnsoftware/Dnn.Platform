import { util } from "utils/helpers";

const promptInit = {
    init() {
        // This setting is required and define the public path 
        // to allow the web application to download assets on demand 
        // eslint-disable-next-line no-undef
        // __webpack_public_path__ = options.publicPath;
        let options = window.dnn.initPrompt();

        util.init(options.utility);
        util.moduleName = options.moduleName;
        util.settings = options.settings;
        util.version = options.version;
        // delay the styles loading after the __webpack_public_path__ is set
        // this allows the fonts associated to be loaded properly in production
        //require("../less/style.less");
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};

export default promptInit;

/* eslint-disable */
export const IS_DEV = process.env.NODE_ENV !== "production";