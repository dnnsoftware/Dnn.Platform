import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import visiblePageSettings from "./visiblePageSettingsReducer";
import pages from "./pagesReducer";
import addPages from "./addPagesReducer";
import theme from "./themeReducer";
import pageHierarchy from "./pageHierarchyReducer";
import errors from "./errorsReducer";
import pageSeo from "./pageSeoReducer";
import extensions from "./extensionsReducer";
import template from "./templateReducer";
import languages from "./languagesReducer";
import pageList from "./pageListReducer";
import searchList from "./searchListReducer";

const rootReducer = combineReducers({
    pageList,
    searchList,
    pagination,
    visiblePanel,
    visiblePageSettings,
    pages,
    addPages,
    theme,
    pageHierarchy,
    errors,
    pageSeo,
    extensions,
    template,
    languages
});

export default rootReducer;
