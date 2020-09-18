// IE8 doesn't like using var dnnModule = dnnModule || {}
if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

dnnModule.DigitalAssetsController = function (servicesFramework, resources, settings) {
    this.servicesFramework = servicesFramework;
    this.resources = resources;
    this.settings = settings;
};

dnnModule.DigitalAssetsController.prototype = function () {

    var getContentServiceUrl = function () {
            return this.servicesFramework.getServiceRoot('DigitalAssets') + 'ContentService/';
        },
        getDownloadServiceUrl = function () {
            return this.servicesFramework.getServiceRoot('DigitalAssets') + 'DownloadService/';
        },
        isDownloadAvailable = function (items) {
            return items.length == 1 && !items[0].IsFolder;
        },
        download = function (items, settings) {
            if (this.isDownloadAvailable.call(this, items)) {
                settings.downloadUrl(getDownloadServiceUrl.call(this) + "Download?fileId=" + items[0].ItemId + "&" + getHttpGETHeaders.call(this));
                return true;
            }
            return false;
        },
        getHttpGETHeaders = function () {
            return "ModuleId=" + this.servicesFramework.getModuleId() + "&TabId=" + this.servicesFramework.getTabId();
        },
        getThumbnailUrl = function (item) {
            return item.IconUrl;
        },
        getThumbnailClass = function (item) {
            return "dnnModuleDigitalAssetsThumbnailNoThumbnail";
        },
        loadContent = function (folderId, startIndex, numItems, sortExpression, settings, scopeId) {
            return false;
        },        
        onLoadFolder = function () {
        },
        executeCommandOnSelectedItems = function (commandName, items) {
        },
        executeCommandOnSelectedNode = function (commandName, node) {
        },
        gridOnGridCreated = function (grid) {
            dnnModule.digitalAssets.loadInitialContent();
        },
        gridOnRowDataBound = function (grid, item) {
        },
        setupGridContextMenuExtension = function (contextMenu, gridItems) {
        },
        setupTreeViewContextMenuExtension = function (contextMenu, node) {
        },
        updateSelectionToolBar = function (selectionToolbar, gridItems) {
        },
        extendResources = function (extendedResouces) {
            $.extend(this.resources, extendedResouces);
        },
        getLeftPaneActions = function() {
            return [];
        },
        updateModuleState = function (stateObject) {
            var state = {};
            state[stateObject.stateMode] = stateObject.stateValue;
            state["view"] = stateObject.currentView;
            state["pageSize"] = stateObject.pageSize;

            var d = new Date();
            d.setDate(d.getDate() + 30);
            document.cookie = "damState-" + stateObject.userId + "=" + encodeURIComponent($.param(state))
                + "; path=" + window.location.pathname
                + "; expires=" + d.toUTCString();

            var deparam = function (str) {
                if (str.length === 0) return {};
                var parts = str.replace(/(^\?)/, '').split("&");
                return $.map(parts, function (n) { return n = n.split("="), this[n[0]] = decodeURIComponent(n[1]), this }.bind({}))[0];
            };

            if (history.replaceState) { // IE9 does not support replaceState
                var p = deparam(window.location.search);
                $.extend(p, state);
                history.replaceState(null, null, '?' + $.param(p));
            }            
        },
        getCurrentState = function (grid,  view) {
            var stateMode = "folderId";
            var stateValue = dnnModule.digitalAssets.getCurrentFolderId();
            return {
                stateMode: stateMode,
                stateValue: stateValue,
                currentView: view,
                pageSize: grid.get_pageSize(),
                userId: this.settings.userId
            };
        },
        leftPaneTabActivated = function(id) {
        },
        initMainMenuButtons = function(settings) {
        };

    return {
        isDownloadAvailable: isDownloadAvailable,
        download: download,
        getContentServiceUrl: getContentServiceUrl,
        getDownloadServiceUrl: getDownloadServiceUrl,
        getThumbnailUrl: getThumbnailUrl,
        getThumbnailClass: getThumbnailClass,
        loadContent: loadContent,
        onLoadFolder: onLoadFolder,
        getHttpGETHeaders: getHttpGETHeaders,
        gridOnGridCreated: gridOnGridCreated,
        gridOnRowDataBound: gridOnRowDataBound,
        extendResources: extendResources,
        executeCommandOnSelectedItems: executeCommandOnSelectedItems,
        setupGridContextMenuExtension: setupGridContextMenuExtension,
        setupTreeViewContextMenuExtension: setupTreeViewContextMenuExtension,
        updateSelectionToolBar: updateSelectionToolBar,
        executeCommandOnSelectedNode: executeCommandOnSelectedNode,
        getLeftPaneActions: getLeftPaneActions,
        updateModuleState: updateModuleState,
        getCurrentState: getCurrentState,
        leftPaneTabActivated: leftPaneTabActivated,
        initMainMenuButtons: initMainMenuButtons
    };
}();
