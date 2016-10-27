import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

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
            permissions: props.extensionBeingEdited.permissions.value,
            desktopModuleId: props.extensionBeingEdited.desktopModuleId.value
        };
    }

    onPermissionsChanged(permissions) {
        const {props, state} = this;

        let newPermissions = Object.assign({}, state.permissions, permissions);
        this.setState({ permissions: newPermissions });
    }

    savePermissions() {
        const {props, state} = this;
        let permissions = Object.assign({}, state.permissions);

        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        extensionBeingUpdated.permissions.value = permissions;

        props.updateExtensionBeingEdited(extensionBeingUpdated, {permissions: JSON.stringify(permissions)}, function () {
            utils.utilities.notify(Localization.get("UpdateComplete"));
        });
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
                    <Button onClick={props.onCancel.bind(this)}>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this, true)}>Save & Close</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this)}>{Localization.get("Save")}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

ModuleSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    extensionBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(ModuleSettings);