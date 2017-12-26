export function formatString() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/[{\[](\d+)[\]}]/gi, function (value, index) {
        console.log(`${value} => ${index}`);
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