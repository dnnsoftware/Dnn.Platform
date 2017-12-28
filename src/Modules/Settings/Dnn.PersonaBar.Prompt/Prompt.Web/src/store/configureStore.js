import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import reducers from "../reducers/promptReducers";
import DevTools from "../containers/DevTools";
import { IS_DEV} from "../globals/promptInit";

export default function configureStore(initialState) {
    if (IS_DEV) {
        return createStore(
            reducers,
            initialState,
            compose(applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant()), DevTools.instrument()));
    } else {
        return createStore(
            reducers,
            initialState,
            compose(applyMiddleware(thunkMiddleware), DevTools.instrument()));
    }
}
