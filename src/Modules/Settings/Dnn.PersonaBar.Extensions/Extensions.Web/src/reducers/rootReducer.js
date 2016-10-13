import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import extension from "./extensionReducer";
import installation from "./installationReducer";
import folder from "./folderReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    extension,
    installation,
    folder
});

export default rootReducer;
