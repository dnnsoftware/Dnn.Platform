import React, { Component } from "react";
import App from "../components/App";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        let culture = window.parent["personaBarSettings"]["culture"];
        return (
            <div className="siteSettings-Root">
                <App cultureCode={culture} />
            </div>
        );
    }
}

export default Root;