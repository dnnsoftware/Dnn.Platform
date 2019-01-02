import React  from "react";

export function formatString() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/[{[](\d+)[\]}]/gi, function (value, index) {
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

export function formatLabel(input) {
    if (typeof input === "string") {
        // format camelcase and remove Is from labels
        let output = input.replace("$", "").replace(/^(Is)(.+)/i, "$2");
        output = output.match(/[A-Z][a-z]+/g).join(" "); // rudimentary but should handle normal Camelcase
        return output;
    }
    return "";
}

export function getColumnsFromRow(row) {
    const columns = [];
    for (let key in row) {
        if (key.indexOf("__") !== 0) {
            columns.push(key);
        }
    }
    return columns;
}

export function renderObject(data, fieldOrder) {

    const columns = !fieldOrder || fieldOrder.length === 0 ? getColumnsFromRow(data) : fieldOrder;
    const rows = columns.map((fldName, index) => {
        const lbl = formatLabel(fldName);
        // explicitly checking for null and undefined to cover case where { data["isDeleted"] : false }
        const fldVal = data[fldName] !== undefined && data[fldName] !== null ? data[fldName].toString() : "";
        const cmd = data["__" + fldName] ? data["__" + fldName] : null;

        if (cmd) {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{fldVal}</a></td></tr>;
        } else {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td>{fldVal}</td></tr>;
        }

    });
    return <table className="dnn-prompt-tbl"><tbody>{rows}</tbody></table>;
}

export function stripWhiteSpaces(html = "") {
    const re = />[^\w^<]+</igm;
    return html.replace(re,"><");
}

export const util = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
    },
    utilities: null
};