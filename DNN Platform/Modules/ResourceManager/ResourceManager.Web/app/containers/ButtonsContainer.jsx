import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import localizeService from "../services/localizeService.js";
import manageFolderTypesPanelActions from "../actions/manageFolderTypesPanelActions";
import addFolderPanelActions from "../actions/addFolderPanelActions";
import addAssetPanelActions from "../actions/addAssetPanelActions";

class ButtonsContainer extends React.Component {
    render() {
        const { hasManageFolderTypesPermission, hasAddFilesPermission, hasAddFoldersPermission, showAddFolderPanel, showAddAssetPanel, showManageFolderTypesPanel } = this.props;

        return (
            <div className="right-container">
                <div>
                    { hasManageFolderTypesPermission ? <a className="rm-button secondary" onClick={showManageFolderTypesPanel} >{ localizeService.getString("ManageFolderTypes") }</a> : null }
                    { hasAddFoldersPermission ? <a className="rm-button secondary" onClick={showAddFolderPanel} >{ localizeService.getString("AddFolder") }</a> : null }
                    { hasAddFilesPermission ? <a className="rm-button primary" onClick={showAddAssetPanel} >{ localizeService.getString("AddAsset") }</a> : null }
                </div>
            </div>
        );
    }
}

ButtonsContainer.propTypes = {
    showManageFolderTypesPanel: PropTypes.func,
    showAddFolderPanel: PropTypes.func,
    showAddAssetPanel: PropTypes.func,
    hasManageFolderTypesPermission: PropTypes.bool,
    hasAddFilesPermission: PropTypes.bool,
    hasAddFoldersPermission: PropTypes.bool
};

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            showManageFolderTypesPanel: manageFolderTypesPanelActions.showPanel, 
            showAddFolderPanel: addFolderPanelActions.showPanel,
            showAddAssetPanel: addAssetPanelActions.showPanel
        }, dispatch)
    };
}

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const moduleState = state.module;
    return {
        hasManageFolderTypesPermission: moduleState.isAdmin,
        hasAddFilesPermission: folderPanelState.hasAddFilesPermission,
        hasAddFoldersPermission: folderPanelState.hasAddFoldersPermission
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ButtonsContainer);