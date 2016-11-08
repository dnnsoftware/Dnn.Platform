import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import pages from "./pagesReducer";
import theme from "./themeReducer";
import pageHierarchy from "./pageHierarchyReducer";
import errors from "./errorsReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    pages,
    theme,
    pageHierarchy,
    errors
});

export default rootReducer;
