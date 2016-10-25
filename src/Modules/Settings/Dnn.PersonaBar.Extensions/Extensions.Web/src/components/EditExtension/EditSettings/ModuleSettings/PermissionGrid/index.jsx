import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Label from "dnn-label";
import Grid from "./Grid";
import styles from "./style.less";

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
    allGroupsText: "[All Roles]"
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

    componentWillMount() {
        const {props, state} = this;
    }

    componentWillReceiveProps(newProps){
        this.setState({
            definitions: newProps.permissions.permissionDefinitions,
            rolePermissions: newProps.permissions.rolePermissions,
            userPermissions: newProps.permissions.userPermissions
        });
    }

    localize(key) {
        const {props, state} = this;

        return state.localization[key] || key;
    }
    
    renderRolesGrid(){
        const {props, state} = this;

        return  <Grid 
                    service={props.service} 
                    localization={state.localization} 
                    type="role" 
                    permissions={{definitions: state.definitions, permissions: state.rolePermissions}} />;
    }

    render() {
        const {props, state} = this;

        if (!props.permissions || !props.permissions.permissionDefinitions) {
            return null;
        }

        return (
            <GridCell className={"permissions-grid" + (props.className ? " " + props.className : "")}>
                {this.renderRolesGrid()}
            </GridCell>
        );
    }
}

PermissionGrid.propTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    className: PropTypes.string,
    service: PropTypes.object.isRequired
};

PermissionGrid.DefaultProps = {
};

export default PermissionGrid;