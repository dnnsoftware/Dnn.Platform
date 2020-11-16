import actionTypes from "../action types/messageModalActionsTypes";

const messageModalActions = {
    close() {
        return {
            type: actionTypes.CLOSE_MESSAGE_MODAL
        };
    }
};

export default messageModalActions;