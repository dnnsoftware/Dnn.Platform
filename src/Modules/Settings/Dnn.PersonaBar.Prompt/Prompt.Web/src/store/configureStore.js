import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import reducers from "../reducers/promptReducers";
import DevTools from "../containers/DevTools";

export default function configureStore(initialState) {
    const store = createStore(
        reducers,
        initialState,
        compose(
            applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant()),    // TODO: apply only for development
            DevTools.instrument()
        )
    );
    return store;
}
