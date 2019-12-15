﻿import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import App from "./containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;
application.init();

const appContainer = document.getElementById("security-container");
render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);