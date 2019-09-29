import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import webTab from "./webTabReducer";
import server from "./serverReducer";
import applicationTab from "./applicationTabReducer";
import databaseTab from "./databaseTabReducer";
import smtpServer from "./smtpServerTabReducer";
import logsTab from "./logsTabReducer";
import performanceTab from "./performanceTabReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    webTab,
    server,
    applicationTab,
    databaseTab,
    smtpServer,
    logsTab,
    performanceTab
});

export default rootReducer;
