import actionTypes from "../action types/topBarActionsTypes";

const topBarActions = {
    changeSearchField(text) {
        return {
            type: actionTypes.CHANGE_SEARCH_FIELD,
            data: text
        };
    }
};

export default topBarActions;