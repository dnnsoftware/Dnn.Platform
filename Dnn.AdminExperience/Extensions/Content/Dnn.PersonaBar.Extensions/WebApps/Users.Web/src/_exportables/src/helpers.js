/* eslint-disable no-useless-escape */
import utilities from "utils";
import Moment from "moment";

export function formatDate(dateValue, longformat) {
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
    let yearValue = date.getFullYear();
    if (yearValue < 1900) {
        return "-";
    }

    return Moment(dateValue).locale(utilities.getCulture()).format(longformat === true ? "LLL" : "L");
}

export function validateEmail(value) {
    const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(value);
}

export function formatString() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
        return methodsArgs[argsIndex];
    });
}
export function sort(items, column, order) {
    order = order === undefined ? "asc" : order;
    items = items.sort(function (a, b) {
        if (a[column] > b[column]) //sort string descending
            return order === "asc" ? 1 : -1;
        if (a[column] < b[column])
            return order === "asc" ? -1 : 1;
        return 0;//default return value (no sorting)
    });
    return items;
}

//Reducer helpers
export function updateUsersList(userList, userDetails) {
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    let userDetailsCopy = Object.assign({}, utilities.getObjectCopy(userDetails));

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
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));


    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userId;
        });
    }
    return userListCopy;
}
export function updateUser(userList, userId, deleteStatus, authorizeStatus, superUserStatus) {
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            if (user.userId === userId) {
                if (deleteStatus !== undefined && deleteStatus !== null) {
                    user.isDeleted = deleteStatus;
                }
                if (authorizeStatus !== undefined && authorizeStatus !== null) {
                    user.authorized = authorizeStatus;
                }
                if (superUserStatus !== undefined && superUserStatus !== null) {
                    user.isSuperUser = superUserStatus;
                }
            }
            return true;
        });
    }
    return userListCopy;
}
export function removeUserRoleFromList(userRoles, roleId) {
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));

    if (userRolesCopy.some(role => role.roleId === roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
            return role.roleId !== roleId;
        });
    }
    return userRolesCopy;
}
export function updateUserRoleList(userRoles, roleUserDetails) {
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));
    let roleUserDetailsCopy = Object.assign({}, utilities.getObjectCopy(roleUserDetails));

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
