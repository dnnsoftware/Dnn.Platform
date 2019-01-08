import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import Button from "../Button";
import Label from "../Label";
import Grid from "./Grid";
import "./style.less";

const defaultLocalization = {
    filterByGroup: "Filter By Group:",
    permissionsByRole: "Permissions By Role",
    permissionsByUser: "Permissions By User",
    addRolePlaceHolder: "Begin typing to add a role",
    addUserPlaceHolder: "Begin typing to add a user",
    addRole: "Add",
    addUser: "Add",
    emptyRole: "Add a role to set permissions by role",
    emptyUser: "Add a user to set permissions by user",
    globalGroupsText: "[Global Roles]",
    allGroupsText: "[All Roles]",
    roleText: "Role",
    userText: "User"
};

class PermissionGrid extends Component {
    constructor(props) {
        super(props);

        this.state = {
            definitions: props.permissions.permissionDefinitions,
            rolePermissions: props.permissions.rolePermissions,
            userPermissions: props.permissions.userPermissions,
            localization: Object.assign({}, defaultLocalization, props.localization)
        };
    }

    componentDidUpdate(prevProps) {
        if (this.props.permissions !== prevProps.permissions) {
            this.setState({
                definitions: this.props.permissions.permissionDefinitions,
                rolePermissions: this.props.permissions.rolePermissions,
                userPermissions: this.props.permissions.userPermissions
            });
        }
    }

    localize(key) {
        const {props, state} = this;

        return state.localization[key] || key;
    }

    onPermissionsChanged(type, permissions) {
        const {props, state} = this;

        let newState = {rolePermissions: state.rolePermissions, userPermissions: state.userPermissions};
        switch (type) {
            case "role":
                newState = Object.assign(newState, {rolePermissions: permissions});
                break;
            case "user":
                newState = Object.assign(newState, {userPermissions: permissions});
                break;
        }

        this.setState(newState, function () {
            if (typeof props.onPermissionsChanged === "function") {
                props.onPermissionsChanged(newState);
            }
        });
    }

    onAddPermission(type, permission) {
        const {props, state} = this;

        let newState = {rolePermissions: state.rolePermissions, userPermissions: state.userPermissions};
        switch (type) {
            case "role":
                newState.rolePermissions.push(permission);
                break;
            case "user":
                newState.userPermissions.push(permission);
                break;
        }

        this.setState(newState, function () {
            if (typeof props.onPermissionsChanged === "function") {
                props.onPermissionsChanged(newState);
            }
        });
    }
    
    renderRolesGrid() {
        const {props, state} = this;

        return  <Grid 
            service={props.service} 
            localization={state.localization} 
            type="role" 
            definitions={state.definitions}
            permissions={state.rolePermissions}
            onChange={this.onPermissionsChanged.bind(this, "role")}
            onAddPermission={this.onAddPermission.bind(this, "role")} />;
    }

    renderUsersGrid() {
        const {props, state} = this;

        return  <Grid 
            service={props.service} 
            localization={state.localization} 
            type="user" 
            definitions={state.definitions}
            permissions={state.userPermissions}
            onChange={this.onPermissionsChanged.bind(this, "user")}
            onAddPermission={this.onAddPermission.bind(this, "user")} />;
    }

    render() {
        const {props, state} = this;

        if (!props.permissions || !props.permissions.permissionDefinitions) {
            return null;
        }

        return (
            <GridCell className={"permissions-grid" + (props.className ? " " + props.className : "")}>
                {this.renderRolesGrid()}
                {this.renderUsersGrid()}
            </GridCell>
        );
    }
}

PermissionGrid.propTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    className: PropTypes.string,
    service: PropTypes.object.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired
};

PermissionGrid.DefaultProps = {
};

export default PermissionGrid;