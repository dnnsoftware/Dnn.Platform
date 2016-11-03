import serviceFramework from "./serviceFramework";

const PageService = function () {

    const getPage = function (pageId) {
        return serviceFramework.get("GetPageDetails", { pageId });
    };

    const savePage = function (page) {
        return serviceFramework.post("SavePageDetails", page);
    };

    const getNewPage = function () {
        return {
            tabId: 0,
            name: "",
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
            createdOnDate: "",
            placeholderURL: "/",
            modules: [],
            permissions: {
                permissionDefinitions: [],
                rolePermissions: [],
                userPermissions: []
            }
        };
    };

    return {
        getPage,
        savePage,
        getNewPage
    };
};

const pageService = PageService();
export default pageService;