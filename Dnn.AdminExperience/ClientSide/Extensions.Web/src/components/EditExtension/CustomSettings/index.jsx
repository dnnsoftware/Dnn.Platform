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
        switch (props.type) {
            case "Auth_System":
                return !props.isAddMode && <EditAuthenticationSystem {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "SkinObject":
                return !props.isAddMode && <SkinObject {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "Skin":
            case "Container":
                return !props.isAddMode && <Container {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "ExtensionLanguagePack":
                return <ExtensionLanguagePack {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "CoreLanguagePack":
                return <CoreLanguagePack {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "JavaScript_Library":
                return !props.isAddMode && <JavascriptLibrary {...props} className={props.isAddMode ? "add-mode": ""} />;
            case "Module":
                return <Module {...props} className={props.isAddMode ? "add-mode": ""} />;
            default:
                return <div></div>;
        }
    }

    render() {
        const {props} = this;
        return this.getExtensionSetting(props);
    }
}

export default CustomSettings;