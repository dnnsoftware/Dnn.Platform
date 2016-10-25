import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import pages from "./pagesReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    pages
});

export default rootReducer;
