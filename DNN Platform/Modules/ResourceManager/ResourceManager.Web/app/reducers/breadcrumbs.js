import globalActionsTypes from "../action types/globalActionsTypes";
import folderPanelActionsTypes from "../action types/folderPanelActionsTypes";

const initialState = {
    breadcrumbs: []
};

export default function breadcrumbsReducer(state = initialState, action) {
    const data = action.data;
    switch (action.type) {
        case globalActionsTypes.MODULE_PARAMETERS_LOADED : {
            const {openFolderId} = data;
            let breadcrumbs = initialState.breadcrumbs;
            if (openFolderId) {
                breadcrumbs = [{folderId: data.homeFolderId}];
            }
            
            return { ...state, breadcrumbs };
        }
        case folderPanelActionsTypes.CONTENT_LOADED : {
            let found = false;
            const { folderId, folderName, folderPath, folderParentId } = data.folder;
            let folderLoaded = { folderId, folderName: !folderName && !folderPath ? "Site Root" : folderName };
            let breadcrumbs = state.breadcrumbs.slice();

            for (let i = 0; i < breadcrumbs.length; i++) {
                let breadcrumb = breadcrumbs[i];
                if (breadcrumb.folderId === folderId) {
                    breadcrumbs = breadcrumbs.slice(0, i);
                    found = true;
                    break;
                }
                else if (breadcrumb.folderId === folderParentId) {
                    breadcrumbs = breadcrumbs.slice(0, i+1);
                    found = true;
                    break;
                }
            }

            if (!found) {
                breadcrumbs = breadcrumbs.slice(0, 1);
            }

            breadcrumbs.push(folderLoaded);
            return { ...state, breadcrumbs }; 
        }
    }
    
    return state;
}