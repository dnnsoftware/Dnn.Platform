import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import webTab from "./webTabReducer";
import server from "./serverReducer";
import applicationTab from "./applicationTabReducer";
import databaseTab from "./databaseTabReducer";
import smtpServer from "./smtpServerTabReducer";
import logsTab from "./logsTabReducer";

const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    webTab,
    server,
    applicationTab,
    databaseTab,
    smtpServer,
    logsTab
});

export default rootReducer;
