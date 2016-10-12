import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import siteInfo from "./siteInfoReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    siteInfo
});

export default rootReducer;
