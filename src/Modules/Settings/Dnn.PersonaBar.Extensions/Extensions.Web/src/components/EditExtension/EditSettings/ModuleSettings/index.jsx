import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { PermissionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import PermissionGrid from "dnn-permission-grid";
import Localization from "localization";
import utils from "utils";
import styles from "./style.less";

class ModuleSettings extends Component {
    constructor() {
        super();

        this.state = {
            permissions: {}
        };
    }

    componentWillMount() {
        const {props, state} = this;
        let desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        props.dispatch(PermissionActions.getDesktopModulePermissions(desktopModuleId));
    }

    onPermissionsChanged(permissions){
        const {props, state} = this;

        state.permissions = permissions;
    }

    savePermissions(){
        const {props, state} = this;
        let desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        let permissions = Object.assign({}, state.permissions, {desktopModuleId: desktopModuleId });

        props.dispatch(PermissionActions.saveDesktopModulePermissions(permissions, function(){
            utils.utilities.notify(Localization.get("UpdateComplete"));
        }));
    }

    render() {
        const {props, state} = this;

        return (
            <GridCell className="module-settings">
                <PermissionGrid 
                    permissions={props.permissions} 
                    service={utils.utilities.sf} 
                    onPermissionsChanged={this.onPermissionsChanged.bind(this)} />
                <GridCell className="actions-row">
                    <Button>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this)}>{Localization.get("Save")}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

ModuleSettings.PropTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        permissions: state.permission.desktopModulePermissions
    };
}

export default connect(mapStateToProps)(ModuleSettings);