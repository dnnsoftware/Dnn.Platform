import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";

import Localization from "localization";

const nameColumnSpace = 15;
const actionColumnSpace = 10;

class GridRow extends Component {

    getRowSetup() {
        const { props } = this;
        const {permissionDefinitions} = props,
            permissionDefinitionLength = permissionDefinitions.length,
            gridRowSpace = (100 - (nameColumnSpace + actionColumnSpace)) / permissionDefinitionLength;
        return permissionDefinitions.map((permissionDefinition) => {
            return <GridCell columnSize={gridRowSpace} >
                HEY
            </GridCell>;
        });
    }

    render() {
        const { props } = this;
        const { permission } = props;
        return <GridCell>
            <GridCell columnSize={nameColumnSpace} className="role-column">
                {permission.roleName}
            </GridCell>
            {this.getRowSetup()}
            <GridCell columnSize={actionColumnSpace} className="action-column">

            </GridCell>
        </GridCell>;
    }
}

GridRow.propTypes = {
    permission: PropTypes.object.isRequired,
    permissionDefinitions: PropTypes.object.isRequired
};


export default GridRow;