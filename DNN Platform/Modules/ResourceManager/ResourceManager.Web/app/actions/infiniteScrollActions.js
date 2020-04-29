import actionTypes from "../action types/infiniteScrollActionsTypes";

const infiniteScrollActions = {
    setMaxScrollTop(maxScrollTop) {
        return {
            type: actionTypes.SET_MAX_SCROLL_TOP,
            data: maxScrollTop
        };
    }
};

export default infiniteScrollActions;
