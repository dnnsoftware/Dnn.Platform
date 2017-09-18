import ActionTypes from "../constants/actionTypes/pageHierarchyActionTypes";

const pageHierarchyActions = {
    setSearchKeyword(searchKeyword) {
        return {
            type: ActionTypes.SET_SEARCH_KEYWORD,
            searchKeyword
        };    
    },

    setItemTemplate(itemTemplate) {
        return {
            type: ActionTypes.SET_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    setDragItemTemplate(itemTemplate) {
        return {
            type: ActionTypes.SET_DRAG_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    selectPage(page) {
        return {
            type: ActionTypes.SELECT_PAGE,
            page
        };
    },

    changeSelectedPagePath(path) {
        return {
            type: ActionTypes.CHANGE_SELECTED_PAGE_PATH,
            path
        };
    }
};


export default pageHierarchyActions;