import { combineReducers } from "redux";
import roles from "./rolesReducer";
import roleUsers from "./roleUsersReducer";

const rootReducer = combineReducers({
    roles,
    roleUsers
});

export default rootReducer;
