import React, { Component } from "react";
import AuthenticationSystemSettings from "./AuthenticationSystemSettings";
import ModuleSettings from "./ModuleSettings";
import IFrameHandler from "./IFrameHandler";

class EditSettings extends Component {
    getSettingsEditor(props) {
        switch (props.type) {
            case "Module":
                return <ModuleSettings {...props} />;
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