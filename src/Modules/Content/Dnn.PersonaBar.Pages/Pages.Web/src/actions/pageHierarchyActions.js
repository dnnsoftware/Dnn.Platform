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
    }
};

export default pageHierarchyActions;