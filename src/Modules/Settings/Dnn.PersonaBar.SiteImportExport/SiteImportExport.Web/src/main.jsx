import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "./globals/application";
import configureStore from "./store/configureStore";
import itemsToExportService from "./services/itemsToExportService";
import App from "./containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;
application.init();

if (!window.dnn.siteImportExport) {
    window.dnn.siteImportExport = {};
}
window.dnn.siteImportExport.registerItemToExport = itemsToExportService.registerItemToExport;

const appContainer = document.getElementById("siteimportexport-container");
render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);