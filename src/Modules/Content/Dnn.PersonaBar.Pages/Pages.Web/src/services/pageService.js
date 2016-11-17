import Api from "./api";

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
        return api.post("SavePageDetails", toBackEndPage(page));
    };

    const addPages = function (bulkPage) {
        const api = getOverridablePagesApi();
        return api.post("SaveBulkPages", bulkPage);
    };

    const deletePageModule = function (module) {
        const api = getPagesApi();
        return api.post("DeletePageModule", module);
    };

    const getPageUrlPreview = function (value) {
        const api = getPagesApi();
        return api.get("GetPageUrlPreview", { url: value });
    };

    const getNewPage = function () {
        const api = getPagesApi();
        return api.get("GetDefaultSettings")
            .then(settings => {
                return {
                    tabId: 0,
                    name: "",
                    status: "Visible",
                    localizedName: "",
                    alias: "",
                    title: "",
                    description: "",
                    keywords: "",
                    tags: "",
                    url: "",
                    externalRedirection: "",
                    fileRedirection: "",
                    existingTabRedirection: "",
                    includeInMenu: true,
                    allowIndex: true,
                    thumbnail: "",
                    created: "",
                    hierarchy: "",
                    hasChild: false,
                    type: 0,
                    customUrlEnabled: true,
                    pageType: "normal",
                    isCopy: false,
                    startDate: null,
                    endDate: null,
                    createdOnDate: new Date(),
                    placeholderURL: "/",
                    modules: [],
                    permissions: settings.permissions,
                    templates: settings.templates,
                    schedulingEnabled: false,
                    permanentRedirect: false,
                    linkNewWindow: false,
                    templateTabId: null,
                    templateId: settings.templateId
                };
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
    
    const toFrontEndPage = function (page) {
        return {
            ...page,
            schedulingEnabled: page.startDate || page.endDate 
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

    return {
        getPage,
        savePage,
        addPages,
        getNewPage,
        getCacheProviderList,
        deletePageModule,
        getPageUrlPreview,
        copyAppearanceToDescendantPages,
        copyPermissionsToDescendantPages
    };
};

const pageService = PageService();
export default pageService;
