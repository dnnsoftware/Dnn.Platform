import {roleUsers as ActionTypes}  from "../constants/actionTypes";
import {
    updateRoleUserList,
    removeRoleUserFromList
} from "../components/roles/helpers/roleUsers";

export default function roleUsersReducer(state = {
    matchedUsers: [],
    roleUsers: [],
    totalRecords: 0
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SUGGEST_LIST:
            return { ...state,
                matchedUsers: action.data.matchedUsers
            };
        case ActionTypes.RETRIEVED_USERS_LIST:
            return { ...state,
                roleUsers: action.data.roleUsers,
                totalRecords: action.data.totalRecords
            };
        case ActionTypes.ADD_USER_INTO_ROLE:
        {
            let roleUsers = Object.assign([], JSON.parse(JSON.stringify(state.roleUsers)));
            return { ...state,
                roleUsers: updateRoleUserList(roleUsers, action.data.roleUserDetails)
            };
        }
        case ActionTypes.REMOVE_USER:
        {
            let roleUsers = Object.assign([], JSON.parse(JSON.stringify(state.roleUsers)));
            return { ...state,
                roleUsers: removeRoleUserFromList(roleUsers, action.data.UserId)
            };
        }
        default:
            return { ...state };
    }
}
