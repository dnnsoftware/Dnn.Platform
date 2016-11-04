import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import pages from "./pagesReducer";
import pageHierarchy from "./pageHierarchyReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    pages,
    pageHierarchy
});

export default rootReducer;
