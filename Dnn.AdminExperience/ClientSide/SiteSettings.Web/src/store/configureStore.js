import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import rootReducer from "../reducers/rootReducer";

 
const IS_PRODUCTION = process.env.NODE_ENV === "production";

export default function configureStore(initialState) {
    const middlewareEnhancer = IS_PRODUCTION
        ? applyMiddleware(thunkMiddleware)
        : applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant());
    const store = createStore(
        rootReducer,
        initialState,
        compose(middlewareEnhancer)
    );
    return store;
}
