import ActionTypes from "../constants/actionTypes/pageHierarchyActionTypes";

export default function  pageHierarchyReducer(state = {
    itemTemplate: "pages-list-item-template",
    searchKeyword: ""
}, action) {
    switch (action.type) {
        case ActionTypes.SET_SEARCH_KEYWORD:
            return { ...state,
                searchKeyword: action.searchKeyword
            };

        case ActionTypes.SET_ITEM_TEMPLATE:
            return { ...state,
                itemTemplate: action.itemTemplate
            };

        default:
            return state;
    }
}