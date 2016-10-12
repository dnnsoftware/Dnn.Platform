import {pagination as ActionTypes}  from "../constants/actionTypes";
export default function pagination(state = {
    tabIndex: 0,
    pageIndex: 0,
    pageSize: 10,
    filter: ""
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_TAB_DATA:
            return { ...state,
                tabIndex: action.payload.index
            };
        case ActionTypes.SEARCH_PORTALS:
            return { ...state,
                filter: action.payload.filter
            };
        default:
            return { ...state
            };
    }
}
