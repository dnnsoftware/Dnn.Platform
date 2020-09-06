import React from "react";
import { PropTypes } from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import localizeService from "../services/localizeService";
import manageFolderTypesPanelActions from "../actions/manageFolderTypesPanelActions";

class ManageFolderTypesPanelContainer extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const {
            expanded,
            hidePanel,
            folderTypes,
            manageFolderTypesState,
            isAdmin,
        } = this.props;

        return isAdmin ? (
                <div className={"top-panel manage-folder-types" + (expanded ? " rm-expanded" : "")} >
                    <p>Test</p>
                </div>
        ) : null;
    }
}

ManageFolderTypesPanelContainer.propTypes = {
    expanded: PropTypes.bool,
    hidePanel: PropTypes.func,
    folderTypes: PropTypes.array,
    manageFolderTypesState: PropTypes.object,
    isAdmin: PropTypes.bool,
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const manageFolderTypesPanelState = state.manageFolderTypesPanel;
    const module = state.module;

    return {
        expanded: manageFolderTypesPanelState.expanded,
        isAdmin: module.isAdmin,
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            hidePanel: manageFolderTypesPanelActions.hidePanel,
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ManageFolderTypesPanelContainer);