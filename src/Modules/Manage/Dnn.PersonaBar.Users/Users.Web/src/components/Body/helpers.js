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
export function deleteUser(userList, userId) {
    let userListCopy = Object.assign([], JSON.parse(JSON.stringify(userList)));
    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            if (user.userId === userId) {
                user.isDeleted = true;
            }
            return true;
        });
    }
    return userListCopy;
}

