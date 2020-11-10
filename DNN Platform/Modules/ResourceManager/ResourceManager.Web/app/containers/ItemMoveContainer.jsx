import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import itemDetailsActions from "../actions/itemDetailsActions";
import localizeService from "../services/localizeService";
import FolderPicker from "../components/FolderSelector/FolderPicker"
import folderPanelActions from "../actions/folderPanelActions";

class ItemMoveContainer extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedFolderId: this.props.folderPanelState.currentFolderId,
            selectedFolderName: this.props.folderPanelState.currentFolderName,
        };
    }

    handleFolderChange(e){
        this.setState({
            ...this.state,
            selectedFolderId: e.key,
            selectedFolderName: e.value,
        });
    }

    handleItemMove(){
        if (this.state.selectedFolderId === this.props.folderPanelState.currentFolderId){
            this.props.cancelMoveItem();
            return;
        }

        if (this.props.itemMoving.isFolder){
            this.props.moveFolder(this.props.itemMoving.folderId, this.state.selectedFolderId);
        }
        else {
            this.props.moveFile(this.props.itemMoving.fileId, this.state.selectedFolderId);
        }
    }

    componentWillUnmount(){
        this.props.loadContent(this.props.folderPanelState, this.state.selectedFolderId);
    }

    render() {
        const { cancelMoveItem, homeFolderId } = this.props;
        return (
            <div className="item-details item-move">
                <h3>{localizeService.getString("MoveItem")}</h3>

                <div className="rm-field">
                    <label htmlFor="newLocation">{localizeService.getString("NewLocation")}</label>
                    <FolderPicker
                        selectedFolder={{key: this.state.selectedFolderId, value: this.state.selectedFolderName}}
                        changeFolder={e => this.handleFolderChange(e)} 
                        homeFolderId={homeFolderId}
                        noFolderSelectedValue={localizeService.getString("NoFolderSelected")} 
                        searchFolderPlaceHolder={localizeService.getString("SearchFolder")}
                    />
                </div>

                <div className="rm-clear"></div>
                <div className="cancel">
                    <a className="rm-button secondary normal" onClick={cancelMoveItem}>{localizeService.getString("Cancel")}</a>
                </div>
                <div className="save">
                    <a className="rm-button primary normal" onClick={() => this.handleItemMove()}>{localizeService.getString("Save")}</a>
                </div>
                <div className="rm-clear"></div>
            </div>
        );
    }
}

ItemMoveContainer.propTypes = {
    itemMoving: PropTypes.object,
    cancelMoveItem: PropTypes.func,
    moveItem: PropTypes.func,
    moveFolder: PropTypes.func,
    moveFile: PropTypes.func,
    loadContent: PropTypes.func,
    folderPanelState: PropTypes.object,
    homeFolderId: PropTypes.number,
};

function mapStateToProps(state) {
    const itemDetailsState = state.itemDetails;
    return {
        folderPanelState: state.folderPanel,
        itemMoving: itemDetailsState.itemMoving,
        homeFolderId: state.module.homeFolderId,
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            cancelMoveItem: itemDetailsActions.cancelMoveItem,
            moveItem: itemDetailsActions.moveItem,
            moveFolder: itemDetailsActions.moveFolder,
            moveFile: itemDetailsActions.moveFile,
            loadContent: folderPanelActions.loadContent,
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ItemMoveContainer);
