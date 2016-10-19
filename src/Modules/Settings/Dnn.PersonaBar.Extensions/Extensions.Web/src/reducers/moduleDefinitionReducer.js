import { moduleDefinition as ActionTypes } from "constants/actionTypes";
export default function moduleDefinition(state = {
    moduleDefinitionBeingEdited: {},
    moduleDefinitionBeingEditedIndex: -1
}, action) {
    switch (action.type) {
        case ActionTypes.EDIT_MODULE_DEFINITION:
            return { ...state,
                moduleDefinitionBeingEdited: action.payload.moduleDefinitionBeingEdited,
                moduleDefinitionBeingEditedIndex: action.payload.moduleDefinitionBeingEditedIndex
            };
        case ActionTypes.CLEAR_EDITED_MODULE_DEFINITION:
            return { ...state,
                moduleDefinitionBeingEdited: null,
                moduleDefinitionBeingEditedIndex: -1
            };
        default:
            return { ...state
            };
    }
}
