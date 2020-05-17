import React, {Component} from "react";
import App from "../components/App";
import DevTools from "./DevTools";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        return (
            <div className="siteInfo-Root">
                <App />
                <DevTools />
            </div>
        );
    }
}

export default Root;