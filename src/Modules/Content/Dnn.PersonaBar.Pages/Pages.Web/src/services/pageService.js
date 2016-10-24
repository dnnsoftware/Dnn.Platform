import serviceFramework from "./serviceFramework";

const PageService = function () {

    const getPage = function (pageId) {
        return serviceFramework.get("GetPageDetails", { pageId });
    };

    return {
        getPage: getPage
    };
};

const pageService = PageService();
export default pageService;