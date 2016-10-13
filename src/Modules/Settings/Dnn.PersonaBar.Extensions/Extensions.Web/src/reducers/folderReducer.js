import { folder as ActionTypes } from "constants/actionTypes";

function addToFolders(value, folders){
    return folders.concat(value).sort();
}

export default function folder(state = {
    ownerFolders: [],
    moduleFolders: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_OWNER_FOLDERS:
            console.log(action.payload);
            return { ...state,
                ownerFolders: action.payload
            };
        case ActionTypes.RETRIEVED_MODULE_FOLDERS:
            return { ...state,
                moduleFolders: action.payload
            };
        case ActionTypes.CREATED_NEW_MODULE_FOLDER:
            return { ...state,
                moduleFolders: addToFolders(action.payload.value, state.moduleFolders)
            };
        case ActionTypes.CREATED_NEW_OWNER_FOLDER:
            return { ...state,
                ownerFolders: addToFolders(action.payload.value, state.ownerFolders)                
            };
        default:
            return { ...state
            };
    }
}
