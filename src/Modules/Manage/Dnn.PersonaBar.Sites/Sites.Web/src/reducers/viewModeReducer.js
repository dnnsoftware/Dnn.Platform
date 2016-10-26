import {viewMode as ActionTypes}  from "../constants/actionTypes";
export default function viewMode(state = {
    mode: "card"
}, action) {
    switch (action.type) {
        case ActionTypes.SET_CARD_VIEW:
            return { ...state,
                mode: "card"
            };
        case ActionTypes.SET_LIST_VIEW:
            return { ...state,
                mode: "list"
            };
        default:
            return state;
    }
}
