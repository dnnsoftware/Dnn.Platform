import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import extension from "./extensionReducer";
import installation from "./installationReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    extension,
    installation
});

export default rootReducer;
