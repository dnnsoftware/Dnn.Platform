import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import RoleGroupFilter from "./RoleGroupFilter";
import PermissionGridRow from "./PermissionGridRow";
import Localization from "localization";
import utils from "utils";
import styles from "./style.less";

const defaultLocalization = {
    filterByGroup: "Filter By Group:"
};

class PermissionGrid extends Component {
    constructor() {
        super();

        this.state = {
            permissions: {}
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }

    resx(key) {
        const {props} = this;

        return (props.localization && props.localization[key]) || defaultLocalization[key];
    }


    render() {
        const {props, state} = this;
        if (!props.permissions) {
            return;
        }
        const permissionRows = props.permissions.rolePermissions.map((permission) => {
            return <PermissionGridRow permission={permission} permissionDefinitions={props.permissions.permissionDefinitions} />;
        });

        return (
            <GridCell>
                {permissionRows}
            </GridCell>
        );
    }
}

PermissionGrid.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object.isRequired,
    localization: PropTypes.object
};

PermissionGrid.DefaultProps = {
};

export default PermissionGrid;