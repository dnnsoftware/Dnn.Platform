import resx from "../../../resources";
export function createRoleGroupOptions(roleGroupList) {
    let roleGroupOptions = [];
    if (roleGroupList !== undefined) {
        roleGroupOptions = roleGroupList.map((item) => {
            return { label: item.name, value: item.id };
        });
    }
    return roleGroupOptions;
}
export function removeFromRolesList(rolesList, roleId) {
    if (rolesList.some(role => role.id === roleId)) {
        rolesList = rolesList.filter(role => {
            return role.id !== roleId;
        });
    }
    return rolesList;
}
export function removeFromRoleGroupList(roleGroups, roleGroupId) {
    if (roleGroups.some(roleGroup => roleGroup.id === roleGroupId)) {
        roleGroups = roleGroups.filter(roleGroup => {
            return roleGroup.id !== roleGroupId;
        });
    }
    return roleGroups;
}
export function updateRolesList(rolesList, roleDetails) {
    if (rolesList.some(role => role.id === roleDetails.id)) {
        rolesList = rolesList.filter(role => {
            return role.id !== roleDetails.id;
        });
    }
    if (!rolesList.some(role => role.id === roleDetails.id)) {
        rolesList = [roleDetails].concat(rolesList);
        rolesList = rolesList.sort(function (a, b) {
            let nameA = a.name;
            let nameB = b.name;
            if (nameA < nameB) //sort string ascending
                return -1;
            if (nameA > nameB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return rolesList;
    }
}
export function updateRoleGroupList(roleGroups, roleGroup) {
    if (roleGroup.id > -1) {
        if (roleGroups.some(group => group.id === roleGroup.id)) {
            roleGroups = roleGroups.filter(group => {
                return group.id !== roleGroup.id;
            });
        }
        roleGroups = roleGroups.filter(group => {
            return (group.id !== -1 && group.id !== -2);
        });
        if (!roleGroups.some(group => group.id === roleGroup.id)) {
            roleGroups = [roleGroup].concat(roleGroups);
            roleGroups = roleGroups.sort(function (a, b) {
                let nameA = a.name;
                let nameB = b.name;
                if (nameA < nameB) //sort string ascending
                    return -1;
                if (nameA > nameB)
                    return 1;
                return 0;//default return value (no sorting)
            });
            roleGroups = [{ id: -2, name: resx.get("AllGroups"), description: resx.get("AllGroups") }, { id: -1, name: resx.get("GlobalRolesGroup"), description: resx.get("AllGroups") }]
                .concat(roleGroups);
            return roleGroups;
        }
    }
}
export function decrementUsersCountFromRoleList(rolesList, roleId) {
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId && roleUser.usersCount > 0) {
                roleUser.usersCount--;
            }
            return roleUser;
        });
    }
    return rolesList;
}
export function incrementUsersCountFromRoleList(rolesList, roleId) {
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId) {
                roleUser.usersCount++;
            }
            return roleUser;
        });
    }
    return rolesList;
}
