import utilities from "utils/applicationSettings";
const Sites = {
    init(initCallback) {
        // This setting is required and define the public path 
        // to allow the web application to download assets on demand 
        // eslint-disable-next-line no-undef
        // __webpack_public_path__ = options.publicPath;
        if (typeof window.dnn[initCallback] === "function") {
            let options = window.dnn[initCallback]();
            utilities.init(options);
        }

        // window.dnn[initCallback] = null;
        // delay the styles loading after the __webpack_public_path__ is set
        // this allows the fonts associated to be loaded properly in production
        require("../less/style.less");
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default Sites;