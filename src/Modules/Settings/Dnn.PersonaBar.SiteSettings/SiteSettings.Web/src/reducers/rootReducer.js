import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import siteInfo from "./siteInfoReducer";
import siteBehavior from "./siteBehaviorReducer";
import languages from "./languagesReducer";
import search from "./searchReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    siteInfo,
    siteBehavior,
    languages,
    search
});

export default rootReducer;
