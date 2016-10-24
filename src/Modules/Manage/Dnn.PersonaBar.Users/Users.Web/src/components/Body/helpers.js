export function updateUsersList(userList, userDetails) {
    if (userList.some(user => user.userId === userDetails.userId)) {
        userList = userList.filter(user => {
            return user.userId !== userDetails.userId;
        });
    }
    if (!userList.some(role => role.userId === userDetails.userId)) {
        userList = [userDetails].concat(userList);
        userList = userList.sort(function (a, b) {
            let createdOnDateA = a.createdOnDate;
            let createdOnDateB = b.createdOnDate;
            if (createdOnDateA > createdOnDateB) //sort string descending
                return -1;
            if (createdOnDateA < createdOnDateB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return userList;
    }
}