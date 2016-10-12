import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import portal from "./portalReducer";
import viewMode from "./viewModeReducer";
const rootReducer = combineReducers({
    pagination,
    portal,
    visiblePanel,
    viewMode
});

export default rootReducer;
