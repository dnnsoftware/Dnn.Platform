import React, {Component, PropTypes} from "react";
import App from "../components/App";
import DevTools from "./DevTools";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        return (
            <div className="boilerplate-root">
                <App/>
                <DevTools />
            </div>
        );
    }
}

Root.PropTypes = {};

export default Root;