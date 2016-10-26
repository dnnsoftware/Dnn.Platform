import {users as ActionTypes}  from "../constants/actionTypes";
import {updateUsersList} from "../components/Body/helpers";
export default function user(state = {
    users: [],
    totalUsers: 0,
    userFilters: [],
    userDetails: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_USERS:
            return { ...state,
                users: action.payload.Results,
                totalUsers: action.payload.TotalResults
            };
        case ActionTypes.UPDATE_USER:
            {
                if (action.payload.Success) {
                    let users = Object.assign([], JSON.parse(JSON.stringify(state.users)));
                    let updatedUser = Object.assign({}, JSON.parse(JSON.stringify(action.payload.Results)));
                    return { ...state,
                        users: updateUsersList(users, updatedUser)
                    };
                }
                return { ...state
                };
            }
        case ActionTypes.CREATE_USER:
            {
                if (action.payload.Success) {
                    let users = Object.assign([], JSON.parse(JSON.stringify(state.users)));
                    let totalUsers = Object.assign(state.totalUsers);
                    let newUser = Object.assign({}, JSON.parse(JSON.stringify(action.payload.Results)));
                    return { ...state,
                        users: updateUsersList(users, newUser),
                        totalUsers: totalUsers + 1
                    };
                }
                return { ...state
                };
            }
        case ActionTypes.RETRIEVED_USER_DETAILS:
            return { ...state,
                userDetails: action.payload
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
