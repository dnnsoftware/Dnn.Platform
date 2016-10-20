import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import App from "./containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;

const appContainer = document.getElementById("extensions-container");
application.init();

render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);