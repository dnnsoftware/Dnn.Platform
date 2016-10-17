import React, { Component } from "react";
import EditAuthenticationSystem from "./AuthenticationSystem";
import Module from "./Module";

class CustomSettings extends Component {
    componentWillMount(){
        const { props } = this;
        props.getPackageSettings();

    }
    getExtensionSetting(props) {
        switch (props.type) {
            case "Auth_System":
                return <EditAuthenticationSystem {...props} />;
            case "Module":
                return <Module {...props} />;
            default:
                return <p>Extension Settings</p>;
        }
    }

    render() {
        const {props} = this;
        return this.getExtensionSetting(props);
    }
}

export default CustomSettings;