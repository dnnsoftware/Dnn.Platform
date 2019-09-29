import { combineReducers } from "redux";
import visiblePanel from "./visiblePanelReducer";
import theme from "./themeReducer";
const rootReducer = combineReducers({
    theme,
    visiblePanel
});

export default rootReducer;
