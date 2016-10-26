import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import portal from "./portalReducer";
import viewMode from "./viewModeReducer";
import portalList from "dnn-sites-portal-list-reducer";

const rootReducer = combineReducers({
    pagination,
    portal,
    visiblePanel,
    viewMode,
    portalList
});

export default rootReducer;
