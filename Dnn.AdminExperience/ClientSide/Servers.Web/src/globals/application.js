import utilities from "../utils";
import "../less/style.less";

const serverTabsList = [];

function init(initCallback) { 
    let options = window.dnn[initCallback]();
    utilities.init(options);
    utilities.getPanelIdFromPath = options.utilities.getPanelIdFromPath;
    utilities.updatePanelTabView = options.utilities.updatePanelTabView;
    utilities.panelViewData = options.utilities.panelViewData;
    utilities.path = options.path;
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