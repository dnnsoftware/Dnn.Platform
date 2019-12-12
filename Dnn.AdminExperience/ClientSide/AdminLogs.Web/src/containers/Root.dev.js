import React, {Component} from "react";
import App from "../components/App";

import DevTools from "./DevTools";

class Root extends Component {
    constructor() {
        super();
    }

    render() {
        return (
            <div className="adminlogs-app personaBar-mainContainer">
                <App />
                <DevTools />
            </div>
        );
    }
}
export default (Root);