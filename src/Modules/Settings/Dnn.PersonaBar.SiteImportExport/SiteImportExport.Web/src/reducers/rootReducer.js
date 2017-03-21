import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import jobs from "./jobsReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    jobs
});

export default rootReducer;
