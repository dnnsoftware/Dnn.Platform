import ItemsService from "../services/itemsService";
import actionTypes from "../action types/folderPanelActionsTypes";
import copyToClipboard from "copy-to-clipboard";

function loadingWrap(actionFunction) {
    return (dispatch) => {
        dispatch({
            type: actionTypes.SET_LOADING,
            data: true
        });

        actionFunction()
        .then(() =>
            dispatch({
                type: actionTypes.SET_LOADING,
                data: false
            })
        );
    };
}

function getContent(folderPanelState, changeToFolderId=null) {
    const { numItems, sorting, currentFolderId, loadedItems } = folderPanelState;
    const folderId = changeToFolderId || currentFolderId;
    const startIndex = changeToFolderId ? 0 : loadedItems;

    return dispatch =>
        ItemsService.getContent(folderId, startIndex, numItems, sorting)
        .then(
            response => dispatch({
                type: startIndex ? actionTypes.MORE_CONTENT_LOADED : actionTypes.CONTENT_LOADED,
                data: response
            }),
            reason => dispatch({
                type: actionTypes.LOAD_CONTENT_ERROR,
                data: reason.data ? reason.data.message : null
            })
        );
}

const folderPanelActions = {
    loadContent(folderPanelState, changeToFolderId=null) {
        return (dispatch) => {
            let getContentLogic = () => dispatch(getContent(folderPanelState, changeToFolderId));
            dispatch(loadingWrap(getContentLogic));

            let currentHistoryFolderId = history.state ? history.state.folderId : null;

            if (changeToFolderId && currentHistoryFolderId !== changeToFolderId && changeToFolderId !== folderPanelState.homeFolderId) {
                history.pushState({folderId: changeToFolderId}, null, "?folderId=" + changeToFolderId);
            }
            else if (changeToFolderId && currentHistoryFolderId !== changeToFolderId) {
                let url = window.location.toString();
                if (url.indexOf("?") > 0) {
                    let clean_url = url.substring(0, url.indexOf("?"));
                    history.pushState({folderId: changeToFolderId}, null, clean_url);
                }
            }
        };
    },
    deleteFolder(folderToRemoveId, folderPanelState) {
        const { currentFolderId } = folderPanelState;

        return (dispatch) => {
            let deleteFolder = () =>
                ItemsService.deleteFolder(folderToRemoveId)
                .then(
                    () => dispatch(getContent(folderPanelState, currentFolderId)),

                    reason => dispatch({
                        type: actionTypes.DELETE_FOLDER_ERROR,
                        data: reason.data ? reason.data.message : null
                    })
                );

            dispatch(loadingWrap(deleteFolder));
        };
    },
    deleteFile(fileToRemoveId, folderPanelState) {
        const { currentFolderId } = folderPanelState;

        return (dispatch) => {
            let deleteFile = () =>
                ItemsService.deleteFile(fileToRemoveId)
                .then(
                    () => dispatch(getContent(folderPanelState, currentFolderId)),

                    reason => dispatch({
                        type: actionTypes.DELETE_FILE_ERROR,
                        data: reason.data ? reason.data.message : null
                    })
                );

            dispatch(loadingWrap(deleteFile));
        };
    },
    downloadFile(fileId) {
        return (dispatch) => {
            let downloadUrl = ItemsService.getDownloadUrl(fileId);
            window.open(downloadUrl, "_blank");
            dispatch ({
                type: actionTypes.FILE_DOWNLOADED
            });
        };
    },
    copyFileUrlToClipboard(item) {
        if (!item.path) {
            return;
        }
        
        let fullUrl = ItemsService.getItemFullUrl(item.path);
        copyToClipboard(fullUrl);

        return {
            type: actionTypes.URL_COPIED_TO_CLIPBOARD,
            data: fullUrl
        };
    },
    changeSearchingValue(searchText) {
        return {
            type: actionTypes.CHANGE_SEARCH,
            data: searchText
        };
    },
    searchFiles(folderPanelState, search, searchMore=false) {
        const { numItems, sorting, currentFolderId, loadedItems } = folderPanelState;
        const folderId = currentFolderId;
        const startIndex = searchMore ? Math.ceil(loadedItems/numItems) + 1 : 1;

        return (dispatch) => {
            let getContent = () =>
                ItemsService.searchFiles(folderId, search, startIndex, numItems, sorting, "")
                .then(
                    response => dispatch({
                        type: searchMore ? actionTypes.MORE_CONTENT_LOADED : actionTypes.FILES_SEARCHED,
                        data: response
                    }),
                    reason => dispatch({
                        type: actionTypes.SEARCH_FILES_ERROR,
                        data: reason
                    })
                );

            dispatch(loadingWrap(getContent));
        };
    },
    changeSorting(sorting) {
        return {
            type: actionTypes.CHANGE_SORTING,
            data: sorting
        };
    }
};

export default folderPanelActions;
