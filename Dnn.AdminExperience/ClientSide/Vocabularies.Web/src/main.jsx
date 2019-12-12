import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import taxonomy from "./globals/taxonomy";
import configureStore from "./store/configureStore";
import App from "./containers/Root";

let store = configureStore();

taxonomy.dispatch = store.dispatch;
taxonomy.init();

const appContainer = document.getElementById("vocabularies-panel");
render(
    <Provider store={store}>
        <App />
    </Provider>,
    appContainer
);