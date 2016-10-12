import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import log from "./logReducer";
import logSettings from "./logSettingsReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    log,
    logSettings
});

export default rootReducer;
