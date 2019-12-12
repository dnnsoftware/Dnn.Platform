import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import licensing from "./licensingReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    licensing
});

export default rootReducer;
