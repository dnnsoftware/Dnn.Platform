import {pagination as ActionTypes}  from "../constants/actionTypes";
export default function pagination(state = {
    tabIndex: 0
}, action) {
    switch (action.type) {
        case ActionTypes.LOAD_TAB_DATA:
            return { ...state,
                tabIndex: action.payload.index
            };
        default:
            return state;      
    }
}
