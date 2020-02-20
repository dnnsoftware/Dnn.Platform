import React, { Component } from "react";
import AuthenticationSystemSettings from "./AuthenticationSystemSettings";
import ModuleSettings from "./ModuleSettings";
import ContainerSettings from "../CustomSettings/Container";
import JavaScriptLibrary from "../CustomSettings/JavascriptLibrary";
import SkinObject from "../CustomSettings/SkinObject";
import WrapWithContainer from "./WrapWithContainer";
import CoreLanguagePack from "../CustomSettings/CoreLanguagePack";
import ExtensionLanguagePack from "../CustomSettings/ExtensionLanguagePack";
import IFrameHandler from "./IFrameHandler";

function checkIfKnownAuthenticationProvider(name) {
    switch (name) {
        case "DNN_FacebookAuthentication":
        case "DNN_GoogleAuthentication":
        case "DNN_LiveAuthentication":
        case "DNN_TwitterAuthentication":
            return true;
        default:
            return false;
    }
}

class EditSettings extends Component {
    getSettingsEditor(props) {
        switch (props.type) {
            case "Module":
                return <ModuleSettings {...props} />;
            case "Auth_System":
                if (checkIfKnownAuthenticationProvider(props.extensionBeingEdited.name.value)) {
                    return <AuthenticationSystemSettings {...props} />;
                } else {
                    return <IFrameHandler {...props} />;
                }
            case "Container":
            case "Skin":
                return <WrapWithContainer>
                    <ContainerSettings { ...props} disabled={true} />
                </WrapWithContainer>;
            case "JavaScript_Library":
                return <WrapWithContainer>
                    <JavaScriptLibrary { ...props} disabled={true} />
                </WrapWithContainer>;
            case "SkinObject":
                return <WrapWithContainer>
                    <SkinObject { ...props} disabled={true} />
                </WrapWithContainer>;
            case "CoreLanguagePack":
                return <WrapWithContainer>
                    <CoreLanguagePack { ...props} disabled={true} />
                </WrapWithContainer>;
            case "ExtensionLanguagePack":
                return <WrapWithContainer>
                    <ExtensionLanguagePack { ...props} disabled={true} />
                </WrapWithContainer>;
            default:
                return <IFrameHandler {...props} />;
        }
    }

    render() {
        const {props} = this;

        return this.getSettingsEditor(props);
    }
}

export default EditSettings;