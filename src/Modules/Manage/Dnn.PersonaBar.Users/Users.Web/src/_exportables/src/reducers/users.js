import {users as ActionTypes}  from "../actionTypes";
import {updateUsersList, updateUser, removeUser, updateUserRoleList, removeUserRoleFromList} from "../helpers";
const switchCase = [
    {
        condition: ActionTypes.RETRIEVED_USERS,
        functionToRun: (state, action) => {
            return {
                users: action.payload.Results,
                totalUsers: action.payload.TotalResults
            };
        }
    },
    {
        condition: ActionTypes.UPDATE_USER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                return {
                    users: updateUsersList(state.users, action.payload.Results)
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.CREATE_USER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                let totalUsers = Object.assign(state.totalUsers);
                return {
                    users: updateUsersList(state.users, action.payload.Results),
                    totalUsers: totalUsers + 1
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.DELETE_USER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                return {
                    users: updateUser(state.users, action.payload.userId, true, null)
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.RESTORE_USER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                return {
                    users: updateUser(state.users, action.payload.userId, false, null)
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.USER_MADE_SUPERUSER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                let totalUsers = Object.assign(state.totalUsers);
                return {
                    users: removeUser(state.users, action.payload.userId),
                    totalUsers: totalUsers - 1
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.ERASE_USER,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                let totalUsers = Object.assign(state.totalUsers);
                return {
                    users: removeUser(state.users, action.payload.userId),
                    totalUsers: totalUsers - 1
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.RETRIEVED_USER_DETAILS,
        functionToRun: (state, action) => {
            return {
                userDetails: action.payload
            };
        }
    },
    {
        condition: ActionTypes.RETRIEVED_USERS_ROLES,
        functionToRun: (state, action) => {
            return {
                userRoles: action.payload.UserRoles,
                userRolesCount: action.payload.TotalRecords
            };
        }
    },
    {
        condition: ActionTypes.RETRIEVED_SUGGEST_ROLES,
        functionToRun: (state, action) => {
            return {
                matchedRoles: action.payload.matchedRoles
            };
        }
    },
    {
        condition: ActionTypes.SAVE_USER_ROLE,
        functionToRun: (state, action) => {
            return {
                userRoles: updateUserRoleList(state.userRoles, action.payload)
            };
        }
    },
    {
        condition: ActionTypes.UPDATE_USER_AUTHORIZE_STATUS,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                return {
                    users: updateUser(state.users, action.payload.userId, null, action.payload.authorized)
                };
            }
            return state;
        }
    },
    {
        condition: ActionTypes.REMOVE_USER_ROLE,
        functionToRun: (state, action) => {
            if (action.payload.Success) {
                return {
                    userRoles: removeUserRoleFromList(state.userRoles, action.payload.roleId)
                };
            }
            return state;
        }
    }
];
function getFinalSwitchCase(switchCase, additionalCases) {
    let _switchCase = switchCase;
    if (Object.prototype.toString.call(additionalCases) === "[object Array]") {
        additionalCases.forEach((extraCase) => {
            let alreadyExists = false;
            let indexToChange = 0;
            _switchCase.forEach((item, index) => {
                if (extraCase.condition === item.condition) {
                    alreadyExists = true;
                    indexToChange = index;
                }
            });
            if (!alreadyExists) {
                _switchCase.push(extraCase);
            } else {
                _switchCase[indexToChange] = extraCase;
            }
        });
    }
    return _switchCase;
}
export default function getReducer(initialState, additionalCases) {
    return function common(state = Object.assign({
        users: [],
        totalUsers: 0,
        userFilters: [],
        userRoles: [],
        matchedRoles: [],
        userRolesCount: 0,
        userDetails: {}
    }, initialState), action) {
        let _switchCase = getFinalSwitchCase(switchCase, additionalCases);

        let returnCase = { ...state };

        _switchCase.forEach((to) => {
            if (to.condition === action.type) {
                const stuffToAdd = to.functionToRun(state, action);
                returnCase = Object.assign(returnCase, stuffToAdd);
            }
        });

        return returnCase;
    };
}

