import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import folderPanelActions from "../actions/folderPanelActions";
import AssetsPanelContainer from "./AssetsPanelContainer";
import DialogModalContainer from "./DialogModalContainer";
import MessageModalContainer from "./MessageModalContainer";
import DropZone from "../containers/DropZoneContainer";

class ModuleLayout extends React.Component {
    componentDidMount() {
        const { folderPanelState, loadContent } = this.props;
        this.props.loadContent(folderPanelState);
        
        window.addEventListener("popstate", 
            event => {
                const state = event.state;
                if (state && state.folderId) {
                    loadContent(folderPanelState, state.folderId);
                }
                else {
                    loadContent(folderPanelState, folderPanelState.homeFolderId);
                }
            }
        );
    }

    render() {
        return (
            <div>
                <DropZone disableClick={true} style={{border:"none"}} activeStyle={{border: "10px #0087c6 dashed", padding: "0 0 5px 5px", overflow: "hidden"}}>
                    <AssetsPanelContainer />
                </DropZone>
                <DialogModalContainer />
                <MessageModalContainer />
            </div>
        );
    }
}

ModuleLayout.propTypes = {
    loadContent: PropTypes.func,
    folderPanelState: PropTypes.object
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    return {
        folderPanelState
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            loadContent: folderPanelActions.loadContent
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ModuleLayout);
