import utilities from "../utils";
import { pageHierarchyActions as PageHierarchyActions } from "../actions";
const pageDetailsFooterComponents = [];

const application = {
    init(initCallback) {

        const options = window.dnn[initCallback]();
        utilities.init(options);

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
        application.dispatch(PageHierarchyActions.setItemTemplate(template));
    },
    registerPageDetailFooterComponent(component) {
        pageDetailsFooterComponents.push(component);
    },
    getPageDetailFooterComponents() {
        return pageDetailsFooterComponents;
    }
};

export default application;