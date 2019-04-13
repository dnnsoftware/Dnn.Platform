import {roles as ActionTypes, roleUsers as RoleUserActionTypes}  from "../constants/actionTypes";
import resx from "../resources";
import {
    updateRoleGroupList,
    updateRolesList,
    removeFromRolesList,
    removeFromRoleGroupList,
    decrementUsersCountFromRoleList,
    incrementUsersCountFromRoleList
} from "../components/roles/helpers/roles";

export default function rolesReducer(state = {
    roleGroups: [],
    rolesList: [],
    loadMore: true,
    roleDetails: {},
    roleGroup: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_GROUPS_LIST: {
            let roleGroups = [{ id: -2, name: resx.get("AllGroups"), description: resx.get("AllGroups") }, { id: -1, name: resx.get("GlobalRolesGroup"), description: resx.get("GlobalRolesGroup") }]
                .concat(action.data.roleGroups);
            return { ...state,
                roleGroups: roleGroups
            };
        }
        case ActionTypes.RETRIEVED_ROLES_LIST:
            return { ...state,
                rolesList: action.reload ? action.data.rolesList : state.rolesList.concat(action.data.rolesList),
                loadMore: action.loadMore,
                rsvpLink: action.rsvpLink
            };
        case ActionTypes.UPDATE_ROLEGROUP:
            {
                let roleGroups = Object.assign([], JSON.parse(JSON.stringify(state.roleGroups)));
                return { ...state,
                    roleGroups: updateRoleGroupList(roleGroups, action.data.roleGroup)
                };
            }
        case ActionTypes.UPDATE_ROLE:
            {
                let rolesList = Object.assign([], JSON.parse(JSON.stringify(state.rolesList)));
                if (action.data.currentGroupId === action.data.roleDetails.groupId || action.data.currentGroupId === -2) {

                    return { ...state,
                        rolesList: updateRolesList(rolesList, action.data.roleDetails)
                    };
                }
                return { ...state,
                    rolesList: removeFromRolesList(rolesList, action.data.roleDetails.id)
                };
            }
        case ActionTypes.DELETE_ROLE:
            {
                let rolesList = Object.assign([], JSON.parse(JSON.stringify(state.rolesList)));
                return { ...state,
                    rolesList: removeFromRolesList(rolesList, action.data.roleId)
                };
            }
        case ActionTypes.DELETE_ROLEGROUP:
            {
                let roleGroups = Object.assign([], JSON.parse(JSON.stringify(state.roleGroups)));
                return { ...state,
                    roleGroups: removeFromRoleGroupList(roleGroups, action.data.groupId)
                };
            }
        case RoleUserActionTypes.ADD_USER_INTO_ROLE:
            {
                if (action.data.isAdd) {
                    let rolesList = Object.assign([], JSON.parse(JSON.stringify(state.rolesList)));
                    return { ...state,
                        rolesList: incrementUsersCountFromRoleList(rolesList, action.data.roleUserDetails.roleId)
                    };
                }
                return { ...state };
            }
        case RoleUserActionTypes.REMOVE_USER:
            {
                let rolesList = Object.assign([], JSON.parse(JSON.stringify(state.rolesList)));

                return { ...state,
                    rolesList: decrementUsersCountFromRoleList(rolesList, action.data.RoleId)
                };
            }
        default:
            return { ...state };
    }
}
