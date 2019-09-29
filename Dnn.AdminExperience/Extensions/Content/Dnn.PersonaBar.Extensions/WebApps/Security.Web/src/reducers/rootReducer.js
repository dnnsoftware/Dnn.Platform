import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import security from "./securityReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    security
});

export default rootReducer;
