import serviceFramework from "./serviceFramework";

const PageService = function () {

    const getPage = function (pageId) {
        return serviceFramework.get("GetPageDetails", { pageId });
    };

    const savePage = function (page) {
        return serviceFramework.post("SavePage", { page });
    };

    return {
        getPage,
        savePage
    };
};

const pageService = PageService();
export default pageService;