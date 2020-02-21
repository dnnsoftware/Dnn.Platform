import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import reducers from "reducers/promptReducers";
import DevTools from "containers/DevTools";
import { IS_DEV} from "globals/promptInit";

export default function configureStore(initialState) {
    let enhancer;

    if (IS_DEV) {
        enhancer = compose(applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant()), DevTools.instrument());
    } else {
        enhancer = applyMiddleware(thunkMiddleware);
    }

    return createStore(reducers, initialState, enhancer);
}
