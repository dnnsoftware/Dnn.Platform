import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import webTab from "./webTabReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    webTab
});

export default rootReducer;
