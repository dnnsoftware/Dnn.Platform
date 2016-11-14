import ActionTypes from "../constants/actionTypes/extensionsActionTypes";

const extensionsActions = {
    registerToolbarComponent(component) {
        return {
            type: ActionTypes.REGISTER_TOOLBAR_COMPONENT,
            data: {component}
        };
    }  
};

export default extensionsActions;