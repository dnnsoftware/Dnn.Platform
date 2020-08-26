import actionTypes from "../action types/itemDetailsActionsTypes";
import itemsService from "../services/itemsService";
import localizeService from "../services/localizeService";

const itemDetailsActions = {
    editItem(item) {
        const itemData = {isFolder: item.isFolder, iconUrl: item.iconUrl, thumbnailAvailable: item.thumbnailAvailable, thumbnailUrl: item.thumbnailUrl};
        let getItemDetails = () => item.isFolder
            ? itemsService.getFolderDetails(item.itemId)
            : itemsService.getFileDetails(item.itemId);
        return dispatch => {
            getItemDetails()
                .then(
                    response => dispatch({
                        type: actionTypes.EDIT_ITEM,
                        data: {...response, ...itemData}
                    }),
                    reason => dispatch({
                        type: actionTypes.EDIT_ITEM_ERROR,
                        data: reason.data && reason.data.message ? reason.data.message : localizeService.getString("GenericErrorMessage")
                    })
                );
        };
    },
    cancelEditItem() {
        return {
            type: actionTypes.CANCEL_EDIT_ITEM
        };
    },
    changeName(event) {
        return {
            type: actionTypes.CHANGE_NAME,
            data: event.target.value
        };
    },
    changeTitle(event) {
        return {
            type: actionTypes.CHANGE_TITLE,
            data: event.target.value
        };
    },
    changeDescription(event) {
        return {
            type: actionTypes.CHANGE_DESCRIPTION,
            data: event.target.value
        };
    },
    changePermissions(permissions) {
        return {
            type: actionTypes.CHANGE_PERMISSIONS,
            data: permissions
        };
    },
    setValidationErrors(validationErrors) {
        return {
            type: actionTypes.SET_VALIDATION_ERRORS,
            data: validationErrors
        };
    },
    saveItem(item) {
        let saveFunction = () => item.isFolder
            ? itemsService.saveFolderDetails(item)
            : itemsService.saveFileDetails(item);
        return dispatch => {
            saveFunction()
                .then(
                    () => dispatch({
                        type: actionTypes.ITEM_SAVED,
                        data: {
                            isFolder: item.isFolder,
                            itemId: item.isFolder ? item.folderId : item.fileId,
                            itemName: item.isFolder ? item.folderName : item.fileName
                        }
                    }),
                    reason => dispatch({
                        type: actionTypes.SAVE_ITEM_ERROR,
                        data: reason.data && reason.data.message ? reason.data.message : localizeService.getString("GenericErrorMessage")
                    })
                );
        };
    }
};

export default itemDetailsActions;