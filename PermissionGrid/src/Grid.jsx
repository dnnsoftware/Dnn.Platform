import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import Label from "dnn-label";
import GridCaption from "./GridCaption";
import GridHeader from "./GridHeader";
import GridRow from "./GridRow";

class Grid extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }

    onPermissionChange(permission) {
        const {props, state} = this;

        let permissions = props.permissions.map(function (p) {
            switch (props.type) {
                case "role":
                    if (p.roleId === permission.roleId) {
                        return permission;
                    }

                    return p;
                case "user":
                    if (p.userId === permission.userId) {
                        return permission;
                    }

                    return p;
            }
        });

        if (typeof props.onChange === "function") {
            props.onChange(permissions);
        }
    }

    onPermissionDeleted(permission) {
        const {props, state} = this;

        let permissions = props.permissions.filter(function (p) {
            switch (props.type) {
                case "role":
                    return p.roleId !== permission.roleId;
                case "user":
                    return p.userId !== permission.userId;
            }
        });

        if (typeof props.onChange === "function") {
            props.onChange(permissions);
        }
    }

    onSuggestion(value) {
        const {props, state} = this;

        let permission = props.permissions.filter(function (p) {
            if (props.type === "role") {
                return p.roleId === value.value;
            } else {
                return p.userId === value.value;
            }
        });

        if (!permission.length && typeof props.onAddPermission === "function") {
            let newPermission = props.type === "role" ? {
                roleId: value.value,
                roleName: value.label,
                locked: false,
                default: false,
                permissions: []
            } : {
                    userId: value.value,
                    displayName: value.label,
                    locked: false,
                    default: false,
                    permissions: []
                };
            props.onAddPermission(newPermission);
        }
    }

    renderGrid() {
        const {props, state} = this;

        const totalWidth = 100;
        const defaultColumnWith = 10;
        let roleColumnWidth = 20;
        const minimumActionColumnWidth = 8;

        let columnWidth = defaultColumnWith;
        let actionsWidth;
        const tooManyColumns = totalWidth < roleColumnWidth + props.definitions.length * columnWidth;
        if (tooManyColumns) {
            roleColumnWidth = 13;
            columnWidth = (totalWidth - roleColumnWidth - minimumActionColumnWidth) / props.definitions.length;
            actionsWidth = totalWidth - roleColumnWidth - props.definitions.length * columnWidth;
        } else {
            actionsWidth = totalWidth - roleColumnWidth - props.definitions.length * columnWidth;
        }

        let self = this;
        return <GridCell className={props.type + "-permissions-grid"}>
            <GridCaption service={props.service} localization={props.localization} type={props.type} onSuggestion={this.onSuggestion.bind(this) } />
            <GridHeader type={props.type} definitions={props.definitions}
                roleColumnWidth={roleColumnWidth}
                columnWidth={columnWidth}
                actionsWidth={actionsWidth}
                localization={props.localization}  />
            {props.permissions.map((permission) => {
                return <GridRow
                    type={props.type}
                    definitions={props.definitions}
                    permission={permission}
                    onChange={self.onPermissionChange.bind(self) }
                    onDeletePermisson={self.onPermissionDeleted.bind(self) }
                    roleColumnWidth={roleColumnWidth}
                    columnWidth={columnWidth}
                    actionsWidth={actionsWidth} />;
            }) }
            {props.permissions.length === 0 && <GridCell className="empty-row">{props.type == "role" ? props.localization.emptyRole : props.localization.emptyUser}</GridCell>}
        </GridCell>;
    }

    render() {
        const {props, state} = this;

        return (
            this.renderGrid()
        );
    }
}

Grid.propTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object.isRequired,
    definitions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};

Grid.DefaultProps = {
};

export default Grid;