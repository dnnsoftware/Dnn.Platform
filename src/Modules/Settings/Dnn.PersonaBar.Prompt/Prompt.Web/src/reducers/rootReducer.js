import { combineReducers } from "redux";
import prompt from "./promptReducers";

const rootReducer = combineReducers({
    prompt
});

export default rootReducer;
