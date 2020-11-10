import globalActionsTypes from "../action types/globalActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";
import itemDetailsActionsTypes from "../action types/itemDetailsActionsTypes";

const initialState = {
    loadedItems: 0,
    numItems: 0,
    sorting: "",
    hasAddPermission: false
};

export default function folderPanelReducer(state = initialState, action) {
    const data = action.data;
    switch (action.type) {
        case globalActionsTypes.MODULE_PARAMETERS_LOADED : {
            return { ...state, homeFolderId: data.homeFolderId, currentFolderId: data.openFolderId || data.homeFolderId, 
                numItems: data.numItems, itemWidth: data.itemWidth, sortOptions: data.sortingOptions, sorting: data.sorting };
        }
        case folderPanelActionsTypes.SET_LOADING : {
            return { ...state, loading: data};
        }
        case folderPanelActionsTypes.FILES_SEARCHED : {
            let res = Object.assign({}, state, data);
            delete res.newItem;
            return { ...res, loadedItems: data.items.length };
        }
        case folderPanelActionsTypes.CONTENT_LOADED : {
            let res = Object.assign({}, state, data);
            delete res.newItem;
            delete res.search;
            const {folder, items, hasAddPermission} = data;
            const currentFolderName = !folder.folderName && !folder.folderPath ? "Root" : folder.folderName;
            return { ...res, currentFolderId: folder.folderId, currentFolderName, loadedItems: items.length, hasAddPermission };
        }
        case folderPanelActionsTypes.MORE_CONTENT_LOADED: {
            let items = state.items.slice();
            items = items.concat(data.items);
            return { ...state, items, loadedItems: items.length, hasAddPermission: data.hasAddPermission };
        }
        case folderPanelActionsTypes.CHANGE_SEARCH: {
            return { ...state, search: data };
        }
        case itemDetailsActionsTypes.ITEM_SAVED : {
            let itemUpdated = data;
            let items = state.items.slice();
            let updatedItemIndex = items.findIndex(x => x.isFolder === itemUpdated.isFolder && x.itemId === itemUpdated.itemId);

            items[updatedItemIndex] = { ...items[updatedItemIndex], itemName: itemUpdated.itemName };

            return { ...state, items };
        }
        case folderPanelActionsTypes.CHANGE_SORTING : {
            return { ...state, sorting: data };
        }
    }
    
    return state;
}