import { combineReducers } from "redux";
import { portal, exportPortal, visiblePanel, pagination } from "dnn-sites-common-reducers";


const rootReducer = combineReducers({
    visiblePanel: visiblePanel(),
    portal: portal(),
    exportPortal: exportPortal(),
    pagination: pagination()
});

export default rootReducer;
