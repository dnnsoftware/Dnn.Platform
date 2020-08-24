import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import { PermissionGrid } from "@dnnsoftware/dnn-react-common";

class Permissions extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      permissions: JSON.parse(
        JSON.stringify(props.folderBeingEdited.permissions)
      ),
    };
  }

  onPermissionsChanged(permissions) {
    const { state } = this;

    let newPermissions = Object.assign({}, state.permissions, permissions);
    this.setState({ permissions: newPermissions });
  }

  render() {
    const { props, state } = this;
    
    console.log($.dnnSF());
    const grid = (
      <PermissionGrid
        dispatch={(e) => {}}
        permissions={state.permissions}
        service={$.dnnSF()}
        onPermissionsChanged={this.onPermissionsChanged.bind(this)}
      />
    );
    return <div>{grid}</div>;
  }
}

Permissions.propTypes = {
  updateFolderBeingEdited: PropTypes.func,
  folderBeingEdited: PropTypes.object,
};

function mapStateToProps(state) {
  return {};
}

function mapDispatchToProps(dispatch) {
  return {
    ...bindActionCreators({}, dispatch),
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(Permissions);
