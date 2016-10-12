import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import users from "./usersReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    users
});

export default rootReducer;
