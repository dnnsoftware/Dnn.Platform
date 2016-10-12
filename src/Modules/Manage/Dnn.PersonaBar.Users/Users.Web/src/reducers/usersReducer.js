import {users as ActionTypes}  from "constants/actionTypes";
export default function user(state = {
    users: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_USERS:
            return { ...state,
                users: action.payload.Results
            };
        default:
            return { ...state
            };
    }
}
