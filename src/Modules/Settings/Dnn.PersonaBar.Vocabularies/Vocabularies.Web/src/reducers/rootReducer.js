import { combineReducers } from "redux";
import vocabulary from "./vocabularyReducer";
import vocabularyTermList from "./vocabularyTermListReducer";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
const rootReducer = combineReducers({
    vocabulary,
    pagination,
    vocabularyTermList,
    visiblePanel
});

export default rootReducer;
