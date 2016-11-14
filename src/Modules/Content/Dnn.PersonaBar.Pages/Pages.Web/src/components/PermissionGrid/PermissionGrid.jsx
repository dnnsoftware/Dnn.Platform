import React, {Component, PropTypes} from "react";
import DnnPermissionGrid from "dnn-permission-grid";
import utils from "../../utils";
import cloneDeep from "lodash/cloneDeep";
import style from "./style.less";

class PermissionGrid extends Component {
        
    onPermissionsChanged(permissions) {
        const p = { 
            ...this.props.permissions,
            ...permissions
        };
        this.props.onPermissionsChanged(p);
    }

    render() {
        const serviceFramework = utils.getServiceFramework(); 

        return (
            <div className={style.permissionGrid}>
                <DnnPermissionGrid 
                    permissions={cloneDeep(this.props.permissions)} 
                    onPermissionsChanged={this.onPermissionsChanged.bind(this)}
                    service={serviceFramework} />
                <div style={{clear:"both"}} />
            </div>
        );
    }
}

PermissionGrid.propTypes = {
    permissions: PropTypes.object.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired
};

export default PermissionGrid;