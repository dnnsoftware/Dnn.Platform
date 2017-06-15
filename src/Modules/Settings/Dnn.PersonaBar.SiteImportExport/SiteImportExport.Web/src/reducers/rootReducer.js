import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import importExport from "./importExportReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    importExport
});

export default rootReducer;
