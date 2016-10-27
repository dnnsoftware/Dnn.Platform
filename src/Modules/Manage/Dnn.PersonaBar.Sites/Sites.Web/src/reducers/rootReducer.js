import { combineReducers } from "redux";
import { portal, exportPortal, visiblePanel } from "dnn-sites-common-reducers";


const rootReducer = combineReducers({
    visiblePanel: visiblePanel(),
    portal: portal(),
    exportPortal: exportPortal()
});

export default rootReducer;
