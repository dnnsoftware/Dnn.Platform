import {users as ActionTypes}  from "../constants/actionTypes";
export default function user(state = {
    users: [],
    totalUsers: 0,
    userFilters: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_USERS:
            return { ...state,
                users: action.payload.Results,
                totalUsers: action.payload.TotalResults
            };
        // case ActionTypes.RETRIEVED_USER_FILTERS:
        //     return { ...state,
        //         userFilters: action.payload.Results
        //     };
        default:
            return { ...state
            };
    }
}
