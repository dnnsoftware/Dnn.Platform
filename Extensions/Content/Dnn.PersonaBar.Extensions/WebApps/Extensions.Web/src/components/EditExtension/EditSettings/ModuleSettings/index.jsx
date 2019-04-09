import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions, VisiblePanelActions } from "actions";
import { GridCell, Button, PermissionGrid } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import utils from "utils";
import "./style.less";

class ModuleSettings extends Component {
    constructor(props) {
        super(props);

        this.state = {
            permissions: JSON.parse(JSON.stringify(props.extensionBeingEdited.permissions.value)),
            desktopModuleId: props.extensionBeingEdited.desktopModuleId.value
        };
    }

    onPermissionsChanged(permissions) {
        const {state} = this;

        let newPermissions = Object.assign({}, state.permissions, permissions);
        this.setState({ permissions: newPermissions });
    }

    savePermissions(closeOnSave) {
        const {props, state} = this;
        let permissions = Object.assign({}, state.permissions);

        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        extensionBeingUpdated.permissions.value = permissions;

        let actions = { permissions: JSON.stringify(permissions) };

        props.updateExtensionBeingEdited(extensionBeingUpdated);

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, function () {
            utils.utilities.notify(Localization.get("EditExtension_Notify.Success"));
        }));
        if (closeOnSave) {
            props.dispatch(VisiblePanelActions.selectPanel(0));
            props.dispatch(ExtensionActions.selectEditingTab(0));
        }
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
                    <Button type="primary" onClick={this.savePermissions.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this, false)}>{Localization.get("Save")}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

ModuleSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    updateExtensionBeingEdited: PropTypes.func,
    extensionBeingEdited: PropTypes.object,
    extensionBeingEditedIndex: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(ModuleSettings);
