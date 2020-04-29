import React, { PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import localizeService from "../services/localizeService.js";
import addFolderPanelActions from "../actions/addFolderPanelActions";
import addAssetPanelActions from "../actions/addAssetPanelActions";

class ButtonsContainer extends React.Component {
    render() {
        const { hasAddFilesPermission, hasAddFoldersPermission, showAddFolderPanel, showAddAssetPanel } = this.props;

        return (
            <div className="right-container">
                <div>
                    { hasAddFoldersPermission ? <a className="rm-button secondary" onClick={showAddFolderPanel} >{ localizeService.getString("AddFolder") }</a> : null }
                    { hasAddFilesPermission ? <a className="rm-button primary" onClick={showAddAssetPanel} >{ localizeService.getString("AddAsset") }</a> : null }
                </div>
            </div>
        );
    }
}

ButtonsContainer.propTypes = {
    showAddFolderPanel: PropTypes.func,
    showAddAssetPanel: PropTypes.func,
    hasAddFilesPermission: PropTypes.bool,
    hasAddFoldersPermission: PropTypes.bool
};

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            showAddFolderPanel: addFolderPanelActions.showPanel,
            showAddAssetPanel: addAssetPanelActions.showPanel
        }, dispatch)
    };
}

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;

    return {
        hasAddFilesPermission: folderPanelState.hasAddFilesPermission,
        hasAddFoldersPermission: folderPanelState.hasAddFoldersPermission
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ButtonsContainer);