import React, { Component } from "react";
import App from "../components/App";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        let culture = window.parent["personaBarSettings"]["culture"];
        let portalId = window.parent["personaBarSettings"]["portalId"];
        return (
            <div className="siteSettings-Root">
                <App cultureCode={culture} portalId={portalId} />
            </div>
        );
    }
}

export default Root;