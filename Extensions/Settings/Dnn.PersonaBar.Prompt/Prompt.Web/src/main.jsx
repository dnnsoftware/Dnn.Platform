import React from "react";
import { render } from "react-dom";
import { Provider } from "react-redux";
import application from "globals/promptInit";
import configureStore from "store/configureStore";
import Root from "containers/Root";

let store = configureStore();

application.dispatch = store.dispatch;
application.init();

const appContainer = document.getElementById("dnnPrompt-container");
render(
    <Provider store={store}>
        <Root />
    </Provider>,
    appContainer
);