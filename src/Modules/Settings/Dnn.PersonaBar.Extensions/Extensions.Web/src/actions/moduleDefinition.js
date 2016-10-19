import { moduleDefinition as ActionTypes } from "constants/actionTypes";
const moduleDefinitionActions = {
    editModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
        return dispatch => {
            dispatch({
                type: ActionTypes.EDIT_MODULE_DEFINITION,
                payload: {
                    moduleDefinitionBeingEdited,
                    moduleDefinitionBeingEditedIndex
                }
            });
        };
    },
    clearModuleDefinitionBeingEdited(callback) {
        return dispatch => {
            dispatch({
                type: ActionTypes.CLEAR_EDITED_MODULE_DEFINITION
            });
        };
    }
};

export default moduleDefinitionActions;
