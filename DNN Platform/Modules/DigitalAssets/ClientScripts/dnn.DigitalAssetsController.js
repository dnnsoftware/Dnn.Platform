// IE8 doesn't like using var dnnModule = dnnModule || {}
if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

dnnModule.DigitalAssetsController = function (servicesFramework, resources) {
    this.servicesFramework = servicesFramework;
    this.resources = resources;
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
        loadContent = function (folderId, startIndex, numItems, sortExpression, isHostMenu, scopeId) {
            return false;
        },        
        onLoadFolder = function () {
        },
        gridOnRowDataBound = function (grid, item) {
        },
        extendResources = function (extendedResouces) {
            $.extend(this.resources, extendedResouces);
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
        gridOnRowDataBound: gridOnRowDataBound,
        extendResources: extendResources
    };
}();
