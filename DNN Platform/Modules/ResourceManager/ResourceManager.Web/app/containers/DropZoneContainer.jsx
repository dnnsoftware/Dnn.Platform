import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import DropZone from "react-dropzone";
import addAssetPanelActions from "../actions/addAssetPanelActions";
import api from "../globals/api";

class DropZoneContainer extends React.Component {
  validateFile(file) {
    const {
      maxUploadSize,
      maxFileUploadSizeHumanReadable,
      fileSizeError,
      invalidExtensionError,
    } = this.props;

    if (file.size > maxUploadSize) {
      fileSizeError(file.name, maxFileUploadSizeHumanReadable);
      return false;
    }

    const validExtensions = api
      .getWhitelistObject()
      .extensionWhitelist.split(",");
    const ext = file.name.substr(file.name.lastIndexOf(".") + 1);
    let valid = false;
    for (let i = 0; i < validExtensions.length; i++) {
      const extension = validExtensions[i];
      if (extension !== ext) continue;
      valid = true;
      break;
    }

    if (!valid) {
      invalidExtensionError(file.name);
      return false;
    }

    return true;
  }

  uploadFilesHandler(acceptedFiles) {
    const {
      showPanel,
      folderPanelState,
      trackProgress,
      uploadFiles,
    } = this.props;
    showPanel();
    const validFiles = acceptedFiles.filter(this.validateFile.bind(this));
    uploadFiles(
      validFiles,
      this.getFolderPath(),
      folderPanelState,
      trackProgress
    );
  }

  getFolderPath() {
    const { folder } = this.props.folderPanelState;
    return folder ? folder.folderPath : "";
  }

  render() {
    const { hasPermission, style, activeStyle, className } = this.props;

    return hasPermission ? (
      <DropZone
        onClick={(e) => {
          if (this.props.disableClick) e.preventDefault();
        }}
        style={style}
        activeStyle={activeStyle}
        className={className}
        onDrop={this.uploadFilesHandler.bind(this)}
      >
        {({ getRootProps, getInputProps }) => this.props.children}
      </DropZone>
    ) : (
      <div>{this.props.children}</div>
    );
  }
}

DropZoneContainer.propTypes = {
  children: PropTypes.node,
  disableClick: PropTypes.bool,
  className: PropTypes.string,
  style: PropTypes.any,
  activeStyle: PropTypes.any,
  folderPanelState: PropTypes.object,
  hasPermission: PropTypes.bool,
  showPanel: PropTypes.func,
  uploadFiles: PropTypes.func,
  trackProgress: PropTypes.func,
  maxUploadSize: PropTypes.number,
  maxFileUploadSizeHumanReadable: PropTypes.string,
  fileSizeError: PropTypes.func,
  invalidExtensionError: PropTypes.func,
};

function mapStateToProps(state) {
  const folderPanelState = state.folderPanel;
  const moduleState = state.module;

  return {
    folderPanelState,
    maxUploadSize: moduleState.maxUploadSize,
    maxFileUploadSizeHumanReadable: moduleState.maxFileUploadSizeHumanReadable,
    hasPermission: folderPanelState.hasAddFilesPermission,
  };
}

function mapDispatchToProps(dispatch) {
  return {
    ...bindActionCreators(
      {
        showPanel: addAssetPanelActions.showPanel,
        uploadFiles: addAssetPanelActions.uploadFiles,
        trackProgress: addAssetPanelActions.trackProgress,
        fileSizeError: addAssetPanelActions.fileSizeError,
        invalidExtensionError: addAssetPanelActions.invalidExtensionError,
      },
      dispatch
    ),
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(DropZoneContainer);
