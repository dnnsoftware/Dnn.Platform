import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel
});

export default rootReducer;
