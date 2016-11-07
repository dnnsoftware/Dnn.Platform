import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import siteInfo from "./siteInfoReducer";
import siteBehavior from "./siteBehaviorReducer";
import languages from "./languagesReducer";
import search from "./searchReducer";
import languageEditor from "./languageEditorReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    siteInfo,
    siteBehavior,
    languages,
    search,
    languageEditor
});

export default rootReducer;
