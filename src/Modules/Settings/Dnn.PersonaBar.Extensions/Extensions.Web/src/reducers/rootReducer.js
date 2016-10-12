import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import extension from "./extensionReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    extension
});

export default rootReducer;
