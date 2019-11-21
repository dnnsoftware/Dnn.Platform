import utilities from "../utils";
const serverTabsList = [];

function init(initCallback) {
    // This setting is required and define the public path 
    // to allow the web application to download assets on demand 
    // eslint-disable-next-line no-undef
    // __webpack_public_path__ = options.publicPath;        
    let options = window.dnn[initCallback]();

    utilities.init(options);
    utilities.getPanelIdFromPath = options.utilities.getPanelIdFromPath;
    utilities.updatePanelTabView = options.utilities.updatePanelTabView;
    utilities.panelViewData = options.utilities.panelViewData;
    utilities.path = options.path;

    // delay the styles loading after the __webpack_public_path__ is set
    // this allows the fonts associated to be loaded properly in production
    require("../less/style.less");
}

function dispatch() {
    throw new Error("dispatch method needs to be overwritten from the Redux store");
}

function registerServerTab(serverTab) {
    serverTabsList.push(serverTab);

    let panelId = utilities.getPanelIdFromPath(utilities.path);
    utilities.updatePanelTabView(panelId);
}

function getRegisteredServerTabs() {
    return serverTabsList;
}

const application = {
    init,
    dispatch,
    registerServerTab,
    getRegisteredServerTabs
};


export default application;