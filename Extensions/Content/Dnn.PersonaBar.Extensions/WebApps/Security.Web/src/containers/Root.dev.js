import React, {Component} from "react";
import App from "../components/App";
import DevTools from "./DevTools";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        let culture = window.parent["personaBarSettings"]["culture"];
        return (
            <div className="taskScheduler-Root">
                <App cultureCode={culture} />
                <DevTools />
            </div>
        );
    }
}

export default Root;