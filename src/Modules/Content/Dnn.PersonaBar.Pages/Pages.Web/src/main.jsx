import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import App from "./containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;

window.dnn.pages = window.dnn.pages || {};
window.dnn.pages.apiController = window.dnn.pages.apiController || "Pages";
window.dnn.pages.setItemTemplate = application.setItemTemplate;
window.dnn.pages.registerPageDetailFooterComponent = application.registerPageDetailFooterComponent;

const appContainer = document.getElementById("pages-container");
const initCallback = appContainer.getAttribute("data-init-callback");
application.init(initCallback);

render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);