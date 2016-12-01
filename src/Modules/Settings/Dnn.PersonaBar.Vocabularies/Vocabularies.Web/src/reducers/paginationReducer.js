import {pagination as ActionTypes}  from "../constants/actionTypes";
export default function pagination(state = {
    tabIndex: 0,
    pageIndex: 0,
    scopeTypeId: "*",
    pageSize: 10
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_MORE:
            return { ...state,
                pageIndex: action.payload.pageIndex,
                scopeTypeId: action.payload.scopeTypeId
            };
        case ActionTypes.LOAD_TAB_DATA:
            return { ...state,
                pageIndex: 0,
                tabIndex: action.tabIndex,
                scopeTypeId: action.payload.scopeTypeId
            };
        default:
            return { ...state
            };
    }
}
