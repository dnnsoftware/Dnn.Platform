import React from "react";
import Html from "../components/Html";

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

export function renderObjectFieldCell(data, fieldName, key) {
    const cleanFieldName = fieldName.replace("$", "");
    const value = data[cleanFieldName] !== undefined && data[cleanFieldName] !== null ? data[cleanFieldName] : "";
    const renderedValue = value.isHtml ? <Html html={value.output}/> : value.isHtml === false ? value.output : value.toString();
    const cmd = data["__" + fieldName] ? data["__" + fieldName] : null;

    if (cmd) {
        return <td key={key}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{renderedValue}</a></td>;
    }
    else if (fieldName.indexOf("$") >= 0) {
        return <td key={key} className="mono">--{renderedValue}</td>;
    }
    else {
        return <td key={key}>{renderedValue}</td>;
    }
}

export function renderObject(data, fieldOrder) {

    const columns = !fieldOrder || fieldOrder.length === 0 ? getColumnsFromRow(data) : fieldOrder;
    const rows = columns.map((fieldName, index) => {
        return <tr key={index}>
            <td className="dnn-prompt-lbl">{formatLabel(fieldName)}</td>
            <td>:</td>
            {renderObjectFieldCell(data, fieldName)}
        </tr>;
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