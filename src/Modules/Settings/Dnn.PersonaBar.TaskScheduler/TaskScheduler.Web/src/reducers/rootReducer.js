import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import task from "./taskReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    task
});

export default rootReducer;
