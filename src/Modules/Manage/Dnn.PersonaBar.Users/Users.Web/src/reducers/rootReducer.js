import { combineReducers } from "redux";
import {users} from "_exportables/src/reducers";

const rootReducer = combineReducers({
    users: users()
});

export default rootReducer;
