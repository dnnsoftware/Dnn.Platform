import serviceFramework from "./serviceFramework";

const PageService = function () {

    const getPage = function (pageId) {
        return serviceFramework.get("GetPageDetails", { pageId });
    };

    const savePage = function (page) {
        return serviceFramework.post("SavePageDetails", toBackEndPage(page));
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
            },
            schedulingEnabled: false
        };
    };

    const toFrontEndPage = function (page) {
        return {
            ...page,
            schedulingEnabled: page.StartDate || page.EndDate 
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
        toFrontEndPage
    };
};

const pageService = PageService();
export default pageService;