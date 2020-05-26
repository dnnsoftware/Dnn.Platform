import * as CommonComponents from "./src/Components";
import * as CommonActions from "./src/actions";
import * as CommonReducers from "./src/reducers";
import * as CommonActionTypes from "./src/actionTypes";
if (!window.dnn) {
    window.dnn = {};
}
if (!window.dnn.Sites) {
    window.dnn.Sites = {};
}

window.dnn.Sites.CommonActionTypes = CommonActionTypes; 
window.dnn.Sites.CommonComponents = CommonComponents;
window.dnn.Sites.CommonReducers = CommonReducers;
window.dnn.Sites.CommonActions = CommonActions;