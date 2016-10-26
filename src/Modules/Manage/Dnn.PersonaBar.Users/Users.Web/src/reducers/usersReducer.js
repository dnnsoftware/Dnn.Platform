import {users as ActionTypes}  from "../constants/actionTypes";
import {updateUsersList, deleteUser, removeUser} from "../components/Body/helpers";
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
                    return { ...state,
                        users: updateUsersList(state.users, action.payload.Results)
                    };
                }
                return { ...state
                };
            }
        case ActionTypes.CREATE_USER:
            {
                if (action.payload.Success) {
                    let totalUsers = Object.assign(state.totalUsers);
                    return { ...state,
                        users: updateUsersList(state.users, action.payload.Results),
                        totalUsers: totalUsers + 1
                    };
                }
                return { ...state
                };
            }

        case ActionTypes.DELETE_USER:
            {
                if (action.payload.Success) {
                    return { ...state,
                        users: deleteUser(state.users, action.payload.userId)
                    };
                }
                return { ...state
                };
            }
        case ActionTypes.USER_MADE_SUPERUSER:
        case ActionTypes.ERASE_USER:
            {
                if (action.payload.Success) {
                    let totalUsers = Object.assign(state.totalUsers);
                    return { ...state,
                        users: removeUser(state.users, action.payload.userId),
                        totalUsers: totalUsers - 1
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
            return state;
    }
}
