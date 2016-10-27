export function updateUsersList(userList, userDetails) {
    let userListCopy = Object.assign([], JSON.parse(JSON.stringify(userList)));
    let userDetailsCopy = Object.assign({}, JSON.parse(JSON.stringify(userDetails)));

    if (userListCopy.some(user => user.userId === userDetailsCopy.userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userDetailsCopy.userId;
        });
    }
    if (!userListCopy.some(role => role.userId === userDetailsCopy.userId)) {
        userListCopy = [userDetailsCopy].concat(userListCopy);
        userListCopy = userListCopy.sort(function (a, b) {
            let createdOnDateA = a.createdOnDate;
            let createdOnDateB = b.createdOnDate;
            if (createdOnDateA > createdOnDateB) //sort string descending
                return -1;
            if (createdOnDateA < createdOnDateB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return userListCopy;
    }
}
export function removeUser(userList, userId) {
    let userListCopy = Object.assign([], JSON.parse(JSON.stringify(userList)));


    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userId;
        });
    }
    return userListCopy;
}
export function updateUser(userList, userId, deleteStatus) {
    let userListCopy = Object.assign([], JSON.parse(JSON.stringify(userList)));
    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            if (user.userId === userId) {
                user.isDeleted = deleteStatus;
            }
            return true;
        });
    }
    return userListCopy;
}
export function removeUserRoleFromList(userRoles, roleId, userId) {
    let userRolesCopy = Object.assign([], JSON.parse(JSON.stringify(userRoles)));

    if (userRolesCopy.some(role => role.roleId === roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
            return role.roleId !== roleId;
        });
    }
    return userRolesCopy;
}
export function updateUserRoleList(userRoles, roleUserDetails) {
    let userRolesCopy = Object.assign([], JSON.parse(JSON.stringify(userRoles)));
    let roleUserDetailsCopy = Object.assign({}, JSON.parse(JSON.stringify(roleUserDetails)));

    if (userRolesCopy.some(role => role.roleId === roleUserDetailsCopy.roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
            return role.roleId !== roleUserDetailsCopy.roleId;
        });
    }
    if (!userRolesCopy.some(role => role.roleId === roleUserDetailsCopy.roleId)) {
        userRolesCopy = [roleUserDetailsCopy].concat(userRolesCopy);
        userRolesCopy = userRolesCopy.sort(function (a, b) {
            let roleIdA = a.roleId;
            let roleIdB = b.roleId;
            if (roleIdA < roleIdB) //sort string ascending
                return -1;
            if (roleIdA > roleIdB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return userRolesCopy;
    }
}



