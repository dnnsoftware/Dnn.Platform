import utilities from "../utils";
import PageHierarchyActions from "../actions/pageHierarchyActions";
import ExtensionsActions from "../actions/extensionsActions";
import PageActions from "../actions/pageActions";
import utils from "../utils";
import { languagesActions } from "../actions/index";

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
    load(options) {
        utilities.load(options);
        const viewName = utils.getViewName();

        //Check if page translation is enabled 
        application.dispatch(languagesActions.getContentLocalizationEnabled());

        if (viewName === "edit") {
            application.dispatch(PageActions.loadPage(utils.getCurrentPageId()));
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
    registerInContextMenuComponent(component) {
        application.dispatch(ExtensionsActions.registerInContextMenuComponent(component));
    },
    registerPageSettingsComponent(component) {
        application.dispatch(ExtensionsActions.registerPageSettingsComponent(component));
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
    isSuperUserForPages() {
        return utilities.getIsSuperUser();
    },
    getProductSKU() {
        return utilities.getProductSKU();
    }
};

export default application;