import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import configureStore from "./store/configureStore";
import resourceManager from "./globals/resourceManager";
import App from "./components/App";

import "fetch-ie8";
import "es6-shim";

const store = configureStore();
resourceManager.dispatch = store.dispatch;
resourceManager.render = function (appContainer) {
    render(
        <Provider store={store}>
            <App />
        </Provider>,
        appContainer
    );
};

