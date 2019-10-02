import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import App from "./containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;

window.dnn.server =  {
    registerServerTab: application.registerServerTab
};

const appContainer = document.getElementById("servers-container");
const initCallback = appContainer.getAttribute("data-init-callback");
application.init(initCallback);

render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);