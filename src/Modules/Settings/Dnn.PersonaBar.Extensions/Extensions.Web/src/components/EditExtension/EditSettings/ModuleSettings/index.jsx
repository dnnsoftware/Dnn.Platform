import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import PermissionGrid from "./PermissionGrid";
import Localization from "localization";
import styles from "./style.less";

class ModuleSettings extends Component {
    constructor(){
        super();

        this.state = {
            permissions: {}
        };
    }

    componentWillMount(){
        const {props, state} = this;

        let desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        props.dispatch(ExtensionActions.getDesktopModulePermissions(desktopModuleId));
    }


    render() {
        const {props, state} = this;

        return (
            <GridCell>
                <PermissionGrid permissions={props.permissions} />
            </GridCell>
        );
    }
}

ModuleSettings.PropTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        permissions: state.extension.desktopModulePermissions
    };
}

export default connect(mapStateToProps)(ModuleSettings);