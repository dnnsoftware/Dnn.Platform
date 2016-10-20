import { moduleDefinition as ActionTypes } from "constants/actionTypes";
export default function moduleDefinition(state = {
    formIsDirty: false,
    sourceFolders: [],
    sourceFiles: [],
    icons: []
}, action) {
    switch (action.type) {
        case ActionTypes.SET_FORM_DIRT:
            return { ...state,
                formIsDirty: action.payload
            };
        case ActionTypes.RETRIEVED_SOURCE_FOLDERS:
            return { ...state,
                sourceFolders: action.payload
            };
        case ActionTypes.RETRIEVED_SOURCE_FILES:
            return { ...state,
                sourceFiles: action.payload
            };
        case ActionTypes.RETRIEVED_CONTROL_ICONS:
            return { ...state,
                icons: action.payload
            };
        default:
            return { ...state
            };
    }
}
