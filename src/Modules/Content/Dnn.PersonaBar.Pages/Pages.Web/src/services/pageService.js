import serviceFramework from "./serviceFramework";

const PageService = function () {

    const getPage = function (pageId) {
        return serviceFramework.get("GetPageDetails", { pageId })
            .then(response => toFrontEndPage(response));
    };

    const savePage = function (page) {
        return serviceFramework.post("SavePageDetails", toBackEndPage(page));
    };

    const deletePageModule = function (module) {
        // TODO: Review payload
        return serviceFramework.post("DeletePageModule", module);
    };

    const getPageUrlPreview = function (value) {
        return serviceFramework.get("GetPageUrlPreview", { url: value });
    };

    const getNewPage = function () {
        return serviceFramework.get("GetDefaultPermissions")
            .then(permissions => {
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
                    includeInMenu: true,
                    thumbnail: "",
                    created: "",
                    hierarchy: "",
                    hasChild: false,
                    type: 0,
                    customUrlEnabled: true,
                    pageType: "normal",
                    isCopy: false,
                    trackLinks: false,
                    startDate: null,
                    endDate: null,
                    createdOnDate: new Date(),
                    placeholderURL: "/",
                    modules: [],
                    permissions: permissions,
                    schedulingEnabled: false,
                    permanentRedirect: false,
                    linkNewWindow: false
                };
            });
    };

    const getCacheProviderList = function () {
        return serviceFramework.get("GetCacheProviderList");
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
        getNewPage,
        getCacheProviderList,
        deletePageModule,
        getPageUrlPreview
    };
};

const pageService = PageService();
export default pageService;
