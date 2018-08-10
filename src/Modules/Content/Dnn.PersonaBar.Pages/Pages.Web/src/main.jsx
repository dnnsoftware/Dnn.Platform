import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import "string.prototype.startswith";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import App from "./components/App";

let store = configureStore();

application.dispatch = store.dispatch;

window.dnn.pages = window.dnn.pages || {};
window.dnn.pages.apiController = window.dnn.pages.apiController || "Pages";
window.dnn.pages.setItemTemplate = application.setItemTemplate;
window.dnn.pages.setDragItemTemplate = application.setDragItemTemplate;
window.dnn.pages.registerToolbarComponent = application.registerToolbarComponent;
window.dnn.pages.registerPageSettingsComponent = application.registerPageSettingsComponent;
window.dnn.pages.registerPageDetailFooterComponent = application.registerPageDetailFooterComponent;
window.dnn.pages.registerMultiplePagesComponent = application.registerMultiplePagesComponent;
window.dnn.pages.registerInContextMenuComponent = application.registerInContextMenuComponent;
window.dnn.pages.registerSettingsButtonComponent = application.registerSettingsButtonComponent;
window.dnn.pages.registerPageTypeSelectorComponent = application.registerPageTypeSelectorComponent;
window.dnn.pages.isSuperUserForPages = application.isSuperUserForPages;
window.dnn.pages.getProductSKU = application.getProductSKU;
window.dnn.pages.load = application.load;

const appContainer = document.getElementById("pages-container");
const initCallback = appContainer.getAttribute("data-init-callback");
application.init(initCallback);

render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);