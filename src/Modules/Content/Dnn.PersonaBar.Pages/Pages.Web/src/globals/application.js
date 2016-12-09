import utilities from "../utils";
import PageHierarchyActions from "../actions/pageHierarchyActions";
import ExtensionsActions from "../actions/extensionsActions";

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
    setDragItemTemplate(template) {
        application.dispatch(PageHierarchyActions.setDragItemTemplate(template));
    },
    registerToolbarComponent(component) {        
        application.dispatch(ExtensionsActions.registerToolbarComponent(component));
    },
    registerPageDetailFooterComponent(component) {
        application.dispatch(ExtensionsActions.registerPageDetailFooterComponent(component));
    },    
    registerMultiplePagesComponent(component) {
        application.dispatch(ExtensionsActions.registerMultiplePagesComponent(component));
    },
    registerSettingsButtonComponent(component) {
        application.dispatch(ExtensionsActions.registerSettingsButtonComponent(component));
    },
    registerPageTypeSelectorComponent(component) {
        application.dispatch(ExtensionsActions.registerPageTypeSelectorComponent(component));
    },
    registerAdditionalPanel(panel) {
        application.dispatch(ExtensionsActions.registerAdditionalPanel(panel));
    },
    isSuperUserForPages() {
        return utilities.getIsSuperUser();
    }
};

export default application;