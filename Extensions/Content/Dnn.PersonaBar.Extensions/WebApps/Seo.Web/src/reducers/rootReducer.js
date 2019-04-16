import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import seo from "./seoReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    seo
});

export default rootReducer;
