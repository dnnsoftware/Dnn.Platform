import React, {Component, PropTypes} from "react";
import App from "../components/App";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        return (
            <div className="boilerplate-root">
                <App/>
            </div>
        );
    }
}

Root.PropTypes = {};

export default Root;