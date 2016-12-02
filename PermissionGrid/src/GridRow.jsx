import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import Label from "dnn-label";
import StatusSwitch from "./StatusSwitch";
import IconButton from "./IconButton/IconButton";

class GridRow extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }

    getHeaderColumnText() {
        const {props, state} = this;

        return props.type === "role" ? props.permission.roleName : props.permission.displayName;
    }

    onStatusChanged(def, status) {
        const {props, state} = this;

        let permission = Object.assign({}, props.permission);
        permission.permissions = Object.assign([], props.permission.permissions.filter(p => {
            return p.permissionId !== def.permissionId;
        }));

        if (status > 0) {
            permission.permissions.push(Object.assign({}, def, { allowAccess: status === 1 }));
        }

        this.fixPermissionsStatus(permission, def, status);

        if (typeof props.onChange === "function") {
            props.onChange(permission);
        }
    }

    onDelete() {
        const {props, state} = this;

        if (typeof props.onDeletePermisson === "function") {
            props.onDeletePermisson(props.permission);
        }
    }

    fixPermissionsStatus(permission, def, status) {
        const {props, state} = this;

        if (def.fullControl) {
            if (status === 0) {
                permission.permissions = [];
            } else {
                props.definitions.forEach(function (d) {
                    let id = d.permissionId;
                    let p = permission.permissions.filter(function (c) {
                        return c.permissionId === id;
                    });

                    if (p.length) {
                        p[0].allowAccess = status === 1;
                    } else {
                        permission.permissions.push(Object.assign({}, d, { allowAccess: status === 1 }));
                    }
                });
            }

        } else {
            //Check if View Permission is not allow, then also set other permission
            if (def.view) {
                if (status !== 1) {
                    if (status === 0) {
                        permission.permissions = [];
                    } else {
                        props.definitions.forEach(function (d) {
                            let id = d.permissionId;
                            let p = permission.permissions.filter(function (c) {
                                return c.permissionId === id;
                            });

                            if (p.length) {
                                p[0].allowAccess = status === 1;
                            } else {
                                permission.permissions.push(Object.assign({}, d, { allowAccess: status === 1 }));
                            }
                        });
                    }
                }
            } else {
                //if other permissions are set to true must have View
                if (status === 1) {
                    let permissionName = def.permissionName.toLowerCase();
                    if (permissionName !== 'navigate' && permissionName !== 'browse') {
                        props.definitions.forEach(function (d) {
                            if (!d.view) {
                                return;
                            }

                            let id = d.permissionId;
                            let p = permission.permissions.filter(function (c) {
                                return c.permissionId === id;
                            });

                            if (p.length) {
                                p[0].allowAccess = true;
                            } else {
                                permission.permissions.push(Object.assign({}, d, { allowAccess: true }));
                            }
                        });
                    }
                }
            }

            if (status > 0) {
                let samePermissionsSet = props.definitions.filter(function (d) {
                    if (d.fullControl) {
                        return false;
                    }

                    let p = permission.permissions.filter(function (c) {
                        return c.permissionId === d.permissionId;
                    });

                    if (!p.length) {
                        return true;
                    }

                    let allowAccess = p[0].allowAccess;

                    return (!allowAccess && status === 1) || (allowAccess && status === 2);
                }).length === 0;

                if (samePermissionsSet) {
                    let d = props.definitions.filter(function (d) {
                        return d.fullControl;
                    });

                    if (d.length) {
                        let id = d[0].permissionId;
                        let p = permission.permissions.filter(function (c) {
                            return c.permissionId === id;
                        });

                        if (p.length) {
                            p[0].allowAccess = status === 1;
                        } else {
                            permission.permissions.push(Object.assign({}, d[0], { allowAccess: status === 1 }));
                        }
                    }
                }
            }
        }
    }

    renderRow() {
        const {props} = this;
        const {roleColumnWidth, columnWidth, actionsWidth} = props;
        let self = this;

        return <GridCell className="grid-row">
            <GridCell columnSize={roleColumnWidth}><span title={this.getHeaderColumnText() }>{this.getHeaderColumnText() }</span></GridCell>
            {props.definitions.map(function (def) {
                let permission = props.permission.permissions.filter(p => {
                    return p.permissionId == def.permissionId;
                });

                let status = 0;

                if (props.permission.locked) {
                    status = 3;
                }
                else {
                    status = permission.length > 0 ? (permission[0].allowAccess ? 1 : 2) : 0;
                }

                return <GridCell columnSize={columnWidth}>
                    <StatusSwitch permission={permission} status={status} onChange={self.onStatusChanged.bind(self, def) } />
                </GridCell>;
            }) }
            <GridCell columnSize={actionsWidth} className="col-actions">
                {!props.permission.default && <IconButton type="trash" onClick={this.onDelete.bind(this) } />}
            </GridCell>
        </GridCell>;
    }

    render() {
        const {props, state} = this;

        return (
            this.renderRow()
        );
    }
}

GridRow.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    permission: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"]),
    roleColumnWidth: PropTypes.number.isRequired,
    columnWidth: PropTypes.number.isRequired,
    actionsWidth: PropTypes.number.isRequired
};

GridRow.DefaultProps = {
};

export default GridRow;