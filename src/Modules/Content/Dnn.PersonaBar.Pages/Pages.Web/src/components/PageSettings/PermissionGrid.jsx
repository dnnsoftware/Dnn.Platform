import React, {Component, PropTypes} from "react";
import DnnPermissionGrid from "dnn-permission-grid";
import utils from "../../utils";
import cloneDeep from "lodash/cloneDeep";

class PermissionGrid extends Component {
        
    onPermissionsChanged(permissions) {
        const p = { 
            ...this.props.permissions,
            ...permissions
        };
        this.props.onPermissionsChanged(p);
    }

    render() {
        return (
            <DnnPermissionGrid 
                permissions={cloneDeep(this.props.permissions)} 
                onPermissionsChanged={this.onPermissionsChanged.bind(this)}
                service={utils.utilities.sf} />
        );
    }
}

PermissionGrid.propTypes = {
    permissions: PropTypes.object.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired
};

export default PermissionGrid;