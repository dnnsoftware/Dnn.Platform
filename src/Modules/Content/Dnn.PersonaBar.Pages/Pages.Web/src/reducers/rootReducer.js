import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import pages from "./pagesReducer";
import pageHierarchy from "./pageHierarchyReducer";
import errors from "./errorsReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    pages,
    pageHierarchy,
    errors
});

export default rootReducer;
