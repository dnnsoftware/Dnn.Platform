import {users as ActionTypes}  from "../constants/actionTypes";
import {updateUsersList, updateUser, removeUser, updateUserRoleList, removeUserRoleFromList} from "../components/Body/helpers";
export default function user(state = {
    users: [],
    totalUsers: 0,
    userFilters: [],
    userRoles: [],
    matchedRoles: [],
    userRolesCount: 0,
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
                return state;
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
                return state;
            }

        case ActionTypes.DELETE_USER:
            {
                if (action.payload.Success) {
                    return { ...state,
                        users: updateUser(state.users, action.payload.userId, true, null)
                    };
                }
                return state;
            }
        case ActionTypes.RESTORE_USER:
            {
                if (action.payload.Success) {
                    return { ...state,
                        users: updateUser(state.users, action.payload.userId, false, null)
                    };
                }
                return state;
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
                return state;
            }
        case ActionTypes.RETRIEVED_USER_DETAILS:
            return { ...state,
                userDetails: action.payload
            };
        case ActionTypes.RETRIEVED_USERS_ROLES:
            return { ...state,
                userRoles: action.payload.UserRoles,
                userRolesCount: action.payload.TotalRecords
            };
        case ActionTypes.RETRIEVED_SUGGEST_ROLES:
            return { ...state,
                matchedRoles: action.payload.matchedRoles
            };

        case ActionTypes.SAVE_USER_ROLE:
            return { ...state,
                userRoles: updateUserRoleList(state.userRoles, action.payload)
            };
        case ActionTypes.UPDATE_USER_AUTHORIZE_STATUS: {
            if (action.payload.Success) {
                return { ...state,
                    users: updateUser(state.users, action.payload.userId, null, action.payload.authorized)
                };
            }
            return state;
        }
        case ActionTypes.REMOVE_USER_ROLE: {
            if (action.payload.Success) {
                return { ...state,
                    userRoles: removeUserRoleFromList(state.userRoles, action.payload.roleId, action.payload.UserId)
                };
            }
            return state;
        }
        // case ActionTypes.RETRIEVED_USER_FILTERS:
        //     return { ...state,
        //         userFilters: action.payload.Results
        //     };
        default:
            return state;
    }
}
