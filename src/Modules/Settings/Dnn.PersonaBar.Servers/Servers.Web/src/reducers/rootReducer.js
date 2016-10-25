import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import webTab from "./webTabReducer";
import server from "./serverReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    webTab,
    server
});

export default rootReducer;
