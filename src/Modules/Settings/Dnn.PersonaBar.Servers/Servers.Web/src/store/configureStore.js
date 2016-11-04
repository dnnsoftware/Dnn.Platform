import { createStore, applyMiddleware, compose } from "redux";
import thunk from "redux-thunk";
import rootReducer from "../reducers/rootReducer";

export default function configureStore(initialState) {

    const middleware = process.env.NODE_ENV !== "production" ?
        [require("redux-immutable-state-invariant")(), thunk] :
        [thunk];

    const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

    const store = createStore(
        rootReducer,
        initialState,
        composeEnhancers(applyMiddleware(...middleware))
    );
    return store;
}
