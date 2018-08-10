import * as CommonComponents from "./src/Components";
import * as CommonActions from "./src/actions";
import * as CommonReducers from "./src/reducers";
import * as CommonActionTypes from "./src/actionTypes";
if (!window.dnn) {
    window.dnn = {};
}
if (!window.dnn.Users) {
    window.dnn.Users = {};
}
window.dnn.Users.CommonActionTypes = CommonActionTypes;
window.dnn.Users.CommonComponents = CommonComponents;
window.dnn.Users.CommonReducers = CommonReducers;
window.dnn.Users.CommonActions = CommonActions;