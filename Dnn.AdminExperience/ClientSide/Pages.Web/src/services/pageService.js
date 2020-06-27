import Api from "./api";
import utils from "../utils";
const PageService = function () {

    function getOverridablePagesApi() {
        return new Api(window.dnn.pages.apiController);
    }

    function getPagesApi() {
        return new Api("Pages");
    }

    const getPage = function (pageId) {
        const api = getOverridablePagesApi();
        return api.get("GetPageDetails", { pageId })
            .then(response => toFrontEndPage(response));
    };

    const savePage = function (page) {
        const api = getOverridablePagesApi();            
        let request = { ...page,  url: "" };
        return api.post("SavePageDetails", toBackEndPage(request));
    };

    const deletePage = function (page, hardDelete = false) {
        const api = getPagesApi();
        return api.post("DeletePage?hardDelete=" + hardDelete, { id: page.tabId });
    };

    const addPages = function (bulkPage) {
        const api = getOverridablePagesApi();
        return api.post("SaveBulkPages", bulkPage);
    };

    const validatePages = function (bulkPage) {
        const api = getOverridablePagesApi();
        return api.post("PreSaveBulkPagesValidate", bulkPage);
    };

    const movePage = function (payload) {
        const api = getOverridablePagesApi();
        return api.post("MovePage", payload);
    };

    const deletePageModule = function (module) {
        const api = getPagesApi();
        return api.post("DeletePageModule", module);
    };

    const getNewPage = function (parentPage) {
        const api = getOverridablePagesApi();
        let pageId = 0;
        if (parentPage && typeof parentPage !== "function" && parentPage.id) {
            pageId = parentPage.id;
        }
        return api.get("GetDefaultSettings", { pageId: pageId })
            .then(settings => {
                const page = toFrontEndPage(settings);
                page.tabId = 0;
                page.name = "";
                page.status = "Visible";
                page.localizedName = "";
                page.alias = "";
                page.title = "";
                page.description = "";
                page.keywords = "";
                page.tags = "";
                page.url = "";
                page.externalRedirection = "";
                page.fileRedirection = "";
                page.existingTabRedirection = "";
                page.includeInMenu = true;
                page.allowIndex = true;
                page.thumbnail = "";
                page.created = "";
                page.hasChild = false;
                page.type = 0;
                page.customUrlEnabled = true;
                page.pageType = "normal";
                page.isCopy = false;
                page.startDate = null;
                page.endDate = null;
                page.createdOnDate = new Date();
                page.placeholderURL = "/";
                page.modules = null;
                page.schedulingEnabled = false;
                page.permanentRedirect = false;
                page.linkNewWindow = false;
                page.templateTabId = null;
                page.hasParent = parentPage && typeof parentPage !== "function" && parentPage.id || page.hasParent;
                page.hierarchy = parentPage && typeof parentPage !== "function" && parentPage.id && parentPage.name || page.hierarchy;
                page.parentId = parentPage && typeof parentPage !== "function" && parentPage.id || page.parentId;
                page.iconFile = null;
                page.iconFileLarge = null;
                return page;
            });
    };

    const getCacheProviderList = function () {
        const api = getPagesApi();
        return api.get("GetCacheProviderList");
    };

    const copyAppearanceToDescendantPages = function (pageId, theme) {
        const api = getPagesApi();
        return api.post("CopyThemeToDescendantPages", {
            pageId, theme
        });
    };

    const copyPermissionsToDescendantPages = function (pageId) {
        const api = getPagesApi();
        return api.post("CopyPermissionsToDescendantPages", {
            pageId
        });
    };

    const toFrontEndPage = function (pageResult) {
        return {
            ...pageResult.page,
            schedulingEnabled: pageResult.page.startDate || pageResult.page.endDate,
            validationCode: pageResult.ValidationCode,
        };
    };

    const toBackEndPage = function (page) {
        return {
            ...page,
            startDate: page.schedulingEnabled ? page.startDate : null,
            endDate: page.schedulingEnabled ? page.endDate : null,
            schedulingEnabled: undefined
        };
    };

    const openPageInEditMode = function (id, url, callback) {
        const api = getPagesApi();
        return api.post("EditModeForPage?id=" + id, {})
            .then(() => {
                if (url) {
                    utils.getUtilities().closePersonaBar(function () {
                        window.top.location.href = url;
                    });
                } else if ( typeof callback === "function") {
                    callback();
                }
            });
    };

    const getCachedPageCount = function (cacheProvider, pageId) {
        const api = getPagesApi();
        return api.get("GetCachedItemCount", { cacheProvider: cacheProvider, pageId: pageId });
    };

    const clearCache = function (cacheProvider, pageId) {
        const api = getPagesApi();
        return api.post("ClearCache?cacheProvider=" + cacheProvider + "&pageId=" + pageId);
    };

    const getPageList = () => {
        const api = getOverridablePagesApi();
        return api.get("GetPageList", { searchKey: "" });
    };

    const searchPageList = (searchKey) => {
        const api = getOverridablePagesApi();
        return api.get("GetPageList", { searchKey });
    };

    const searchAndFilterPageList = (params) => {
        const api = getOverridablePagesApi();
        return api.get("SearchPages", params);
    };
    const getChildPageList = (id = "") => {
        const api = getOverridablePagesApi();
        return api.get("GetPageList", { parentId: id });
    };

    const getWorkflowsList = () => {
        const api = getOverridablePagesApi();
        return api.get("GetWorkflows");
    };

    const getPageHierarchy = (id) => {
        const api = getPagesApi();
        return api.get("GetPageHierarchy", {pageId: id});
    };

    return {
        getPageList,
        getChildPageList,
        searchPageList,
        searchAndFilterPageList,
        getPage,
        savePage,
        deletePage,
        addPages,
        validatePages,
        getNewPage,
        getCacheProviderList,
        deletePageModule,
        copyAppearanceToDescendantPages,
        copyPermissionsToDescendantPages,
        openPageInEditMode,
        getCachedPageCount,
        clearCache,
        movePage,
        getWorkflowsList,
        getPageHierarchy
    };
};


const pageService = PageService();
export default pageService;
