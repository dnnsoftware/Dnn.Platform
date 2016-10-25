import React, { Component } from "react";
import EditAuthenticationSystem from "./AuthenticationSystem";
import Module from "./Module";
import Container from "./Container";
import JavascriptLibrary from "./JavascriptLibrary";
import ExtensionLanguagePack from "./ExtensionLanguagePack";
import CoreLanguagePack from "./CoreLanguagePack";
import SkinObject from "./SkinObject";

class CustomSettings extends Component {
    getExtensionSetting(props) {
        console.log(props.type);
        switch (props.type) {
            case "Auth_System":
                return <EditAuthenticationSystem {...props} />;
            case "SkinObject":
                return <SkinObject {...props} />;
            case "Skin":
            case "Container":
                return <Container {...props} />;
            case "ExtensionLanguagePack":
                return <ExtensionLanguagePack {...props} />;
            case "CoreLanguagePack":
                return <CoreLanguagePack {...props} />;
            case "JavaScript_Library":
                return <JavascriptLibrary {...props} />;
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