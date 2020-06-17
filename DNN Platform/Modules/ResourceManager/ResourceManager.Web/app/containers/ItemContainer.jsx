import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import folderPanelActions from "../actions/folderPanelActions";
import itemDetailsActions from "../actions/itemDetailsActions";
import dialogModalActions from "../actions/dialogModalActions";
import Item from "../components/Item";
import itemsService from "../services/itemsService";
import localizeService from "../services/localizeService";

class ItemContainer extends React.Component {
    render() {
        const { item, itemEditing, loadContent, editItem, downloadFile, deleteFolder, 
            deleteFile, copyFileUrlToClipboard, openDialog, closeDialog, 
            folderPanelState, uploadedFiles, newFolderId } = this.props;
        const isFolder = item.isFolder;
        const { hasDeletePermission, hasManagePermission } = folderPanelState;

        function isHighlighted() {
            if (isFolder && newFolderId === item.itemId) {
                return true;
            }

            if (!isFolder) {
                for (let i = 0; i < uploadedFiles.length; i++) {
                    if (uploadedFiles[i].fileId === item.itemId) {
                        return true;
                    }
                }
            }

            return false;
        }

        function getIconUrl() {
            return itemsService.getIconUrl(item);
        }

        function onClickFolder() {
            if (!isFolder) {
                return;
            }

            loadContent(folderPanelState, item.itemId);
        }

        function onEditItem(item, event) {
            event.stopPropagation();
            editItem(item);
        }

        function onDeleteFolder(event) {
            event.stopPropagation();
            if (!isFolder) {
                return;
            }

            const dialogHeader = localizeService.getString("DeleteFolderDialogHeader");
            const dialogMessage = localizeService.getString("DeleteFolderDialogMessage");
            const yesFunction = deleteFolder.bind(this, item.itemId, folderPanelState);
            const noFunction = closeDialog;

            openDialog(dialogHeader, dialogMessage, yesFunction, noFunction);
        }

        function onDeleteFile(event) {
            event.stopPropagation();
            if (isFolder) {
                return;
            }

            const dialogHeader = localizeService.getString("DeleteFileDialogHeader");
            const dialogMessage = localizeService.getString("DeleteFileDialogMessage");
            const yesFunction = deleteFile.bind(this, item.itemId, folderPanelState);
            const noFunction = closeDialog;

            openDialog(dialogHeader, dialogMessage, yesFunction, noFunction);
        }

        function isDetailed() {
            if (!itemEditing) {
                return false;
            }

            const {isFolder, fileId, folderId} = itemEditing;

            if ((isFolder && folderId === item.itemId) ||
                (!isFolder && fileId === item.itemId)) {
                return true;
            }

            return false;
        }

        const handlers = {
            onClick:    isFolder ? onClickFolder : null,
            onEdit:     hasManagePermission ? onEditItem.bind(this, item) : null,
            onCopyToClipboard: isFolder ? null : copyFileUrlToClipboard.bind(this, item),
            onDownload: isFolder ? null : downloadFile.bind(this, item.itemId),
            onDelete:   hasDeletePermission ? isFolder ? onDeleteFolder : onDeleteFile : null
        };

        return (
            <Item item={item} iconUrl={getIconUrl()} handlers={handlers} isHighlighted={isHighlighted()} isDetailed={isDetailed()} />
        );
    }
}

ItemContainer.propTypes = {
    item: PropTypes.object,
    loadContent: PropTypes.func,
    editItem: PropTypes.func,
    downloadFile: PropTypes.func,
    deleteFolder: PropTypes.func,
    deleteFile: PropTypes.func,
    copyFileUrlToClipboard: PropTypes.func,
    openDialog: PropTypes.func,
    closeDialog: PropTypes.func,
    folderPanelState: PropTypes.object,
    uploadedFiles: PropTypes.array,
    newFolderId: PropTypes.number,
    itemEditing: PropTypes.object
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const addAssetPanelState = state.addAssetPanel;
    const addFolderPanelState = state.addFolderPanel;
    const itemDetailsState = state.itemDetails;

    return {
        folderPanelState,
        uploadedFiles: addAssetPanelState.uploadedFiles,
        newFolderId: addFolderPanelState.newFolderId,
        itemEditing: itemDetailsState.itemEditing
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            loadContent: folderPanelActions.loadContent,
            editItem: itemDetailsActions.editItem,
            downloadFile: folderPanelActions.downloadFile,
            deleteFolder: folderPanelActions.deleteFolder,
            deleteFile: folderPanelActions.deleteFile,
            copyFileUrlToClipboard: folderPanelActions.copyFileUrlToClipboard,
            openDialog: dialogModalActions.open,
            closeDialog: dialogModalActions.close
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ItemContainer);