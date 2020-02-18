export function removeRoleUserFromList(roleUsers, userId) {
    if (roleUsers.some(roleGroup => roleGroup.userId === userId)) {
        roleUsers = roleUsers.filter(roleGroup => {
            return roleGroup.userId !== userId;
        });
    }
    return roleUsers;
}
export function updateRoleUserList(roleUsers, roleUserDetails) {
    if (roleUsers.some(role => role.userId === roleUserDetails.userId)) {
        roleUsers = roleUsers.filter(role => {
            return role.userId !== roleUserDetails.userId;
        });
    }
    if (!roleUsers.some(role => role.userId === roleUserDetails.userId)) {
        roleUsers.push(roleUserDetails);
        roleUsers = roleUsers.sort(function (a, b) {
            let userIdA = a.userId;
            let userIdB = b.userId;
            if (userIdA < userIdB) //sort string ascending
                return -1;
            if (userIdA > userIdB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return roleUsers;
    }
}