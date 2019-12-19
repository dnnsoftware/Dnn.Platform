import { combineReducers } from "redux";
import {users} from "dnn-users-common-reducers";

const rootReducer = combineReducers({
    users: users()
});

export default rootReducer;
