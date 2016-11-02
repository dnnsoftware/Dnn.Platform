import utilities from "../utils";
import { pageHierarchyActions as PageHierarchyActions } from "../actions";

const application = {
    init(initCallback) {
        // This setting is required and define the public path 
        // to allow the web application to download assets on demand 
        // eslint-disable-next-line no-undef
        // __webpack_public_path__ = options.publicPath;        
        let options = window.dnn[initCallback]();

        utilities.init(options.utility);

        // delay the styles loading after the __webpack_public_path__ is set
        // this allows the fonts associated to be loaded properly in production
        require("../less/style.less");
        
        if (window.dnn.pages.itemTemplate) {
            application.dispatch(PageHierarchyActions.setItemTemplate(window.dnn.pages.itemTemplate));
        }
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    },
    setItemTemplate(template) {
        console.log("setItemTemplate", template);
        application.dispatch(PageHierarchyActions.setItemTemplate(template));
    }
};

export default application;