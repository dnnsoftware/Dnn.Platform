import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import siteSettings from "./siteSettingsReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    siteSettings
});

export default rootReducer;
