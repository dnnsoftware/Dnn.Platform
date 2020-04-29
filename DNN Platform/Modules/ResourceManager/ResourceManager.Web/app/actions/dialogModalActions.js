import actionTypes from "../action types/dialogModalActionsTypes";

const dialogModalActions = {
    open(dialogHeader, dialogMessage, yesFunction, noFunction) {
        return {
            type: actionTypes.OPEN_DIALOG_MODAL,
            data: {
                dialogHeader, dialogMessage, yesFunction, noFunction
            }
        };
    },
    close() {
        return {
            type: actionTypes.CLOSE_DIALOG_MODAL
        };
    }
};

export default dialogModalActions;
