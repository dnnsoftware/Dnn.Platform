import React, {Component} from "react";
import DevTools from "./DevTools";
import App from "../components/App";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        return (
            <div className="themes-app personaBar-mainContainer">
                <App />
                <DevTools />
            </div>
        );
    }
}


export default Root;