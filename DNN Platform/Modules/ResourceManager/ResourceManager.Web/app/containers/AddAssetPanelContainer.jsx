import React, { PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import FileUpload from "../components/FileUpload";
import localizeService from "../services/localizeService";
import addAssetPanelActions from "../actions/addAssetPanelActions";

class AddAssetPanelContainer extends React.Component {
    render() {
        const { expanded, hasPermission, hidePanel } = this.props;

        return hasPermission ? (
            <div className={"top-panel add-asset" + (expanded ? " rm-expanded" : "")} >
                <FileUpload />

                <div className="close">
                    <a className="rm-button secondary" onClick={hidePanel} >{localizeService.getString("Close")}</a>
                </div>
            </div>
        ) : null;
    }
}

AddAssetPanelContainer.propTypes = {
    expanded: PropTypes.bool,
    hasPermission: PropTypes.bool,
    hidePanel: PropTypes.func
};

function mapStateToProps(state) {
    const addAssetPanelState = state.addAssetPanel;
    const folderPanelState = state.folderPanel;

    return {
        expanded: addAssetPanelState.expanded,
        hasPermission: folderPanelState.hasAddFilesPermission
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            hidePanel: addAssetPanelActions.hidePanel
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(AddAssetPanelContainer);