
import actionTypes from "../action types/globalActionsTypes";

const globalActions = {
    loadInitialParameters(initialConfig) {
        return {
            type: actionTypes.MODULE_PARAMETERS_LOADED,
            data: initialConfig
        };
    }
};

export default globalActions;
