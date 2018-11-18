import React, { Component } from "react";
import App from "components/App";
import DevTools from "containers/DevTools";
import { IS_DEV} from "globals/promptInit";

export default class Root extends Component {
    constructor() {
        super();
    }

    render() {
        return (
            <div className="dnnPrompt-app personaBar-mainContainer">
                <App />
                {IS_DEV && <DevTools />}
            </div>
        );
    }
}