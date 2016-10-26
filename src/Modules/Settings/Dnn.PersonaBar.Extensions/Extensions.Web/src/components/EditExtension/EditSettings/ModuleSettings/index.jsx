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
    constructor(props) {
        super(props);

        this.state = {
            permissions: props.permissions,
            desktopModuleId: props.extensionBeingEdited.desktopModuleId.value
        };
    }

    componentWillMount() {
        const {props, state} = this;

        props.dispatch(PermissionActions.getDesktopModulePermissions(state.desktopModuleId));
    }

    componentWillReceiveProps(newProps){
        
        if(newProps.permissions){
            this.setState({permissions: newProps.permissions});
        }
    }

    onPermissionsChanged(permissions){
        const {props, state} = this;

        let newPermissions = Object.assign({}, state.permissions, permissions);
        this.setState({permissions: newPermissions});
    }

    savePermissions(){
        const {props, state} = this;
        let permissions = Object.assign({}, state.permissions, {desktopModuleId: state.desktopModuleId });

        props.dispatch(PermissionActions.saveDesktopModulePermissions(permissions, function(){
            utils.utilities.notify(Localization.get("UpdateComplete"));
        }));
    }

    render() {
        const {props, state} = this;

        return (
            <GridCell className="module-settings">
                <PermissionGrid 
                    permissions={state.permissions} 
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

ModuleSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object,
    extensionBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
    return {
        permissions: state.permission.desktopModulePermissions
    };
}

export default connect(mapStateToProps)(ModuleSettings);