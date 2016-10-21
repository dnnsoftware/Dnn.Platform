import React, {Component} from "react";
import App from "../components/App";

class Root extends Component {
    constructor() {
        super();
    }
    render() {
        return (
            <div className="pages-root">
                <App/>
            </div>
        );
    }
}

export default Root;