import ActionTypes from "../constants/actionTypes/pageHierarchyActionTypes";
import ContentActionTypes from "../constants/actionTypes/pageActionTypes";

export default function  pageHierarchyReducer(state = {
    itemTemplate: "pages-list-item-template",
    searchKeyword: "",
    selectedPagePath: [],
    selectedPage: null,
    createdPage: null
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

        case ActionTypes.SET_DRAG_ITEM_TEMPLATE:
            return { ...state,
                dragItemTemplate: action.itemTemplate
            };
        
        case ContentActionTypes.SAVED_PAGE:
            return { ...state,
                createdPage: action.data.createdPage
            };

        case ActionTypes.CHANGE_SELECTED_PAGE_PATH:
            return { ...state,
                selectedPagePath: action.path  
            };

        case ActionTypes.SELECT_PAGE:
            return { ...state,
                selectedPage: action.page
            };

        default:
            return state;
    }
}