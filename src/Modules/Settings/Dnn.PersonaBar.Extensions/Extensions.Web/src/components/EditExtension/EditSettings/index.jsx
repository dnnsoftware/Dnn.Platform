import React, { Component } from "react";
import AuthenticationSystemSettings from "./AuthenticationSystemSettings";
import ModuleSettings from "./ModuleSettings";

class EditSettings extends Component {
    getSettingsEditor(props) {
        switch (props.type) {
            case "Auth_System":
                return <AuthenticationSystemSettings {...props} />;
            case "Module":
                return <ModuleSettings {...props} />;
            default:
                return <p>Extension Settings</p>;
        }
    }

    render() {
        const {props} = this;

        return this.getSettingsEditor(props);
    }
}

export default EditSettings;