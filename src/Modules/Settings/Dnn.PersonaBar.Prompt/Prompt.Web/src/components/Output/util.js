import React  from "react";

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
        if (!key.startsWith("__")) {
            columns.push(key);
        }
    }
    return columns;
}

export function renderObject(data, fieldOrder) {

    const columns = !fieldOrder || fieldOrder.length == 0 ? getColumnsFromRow(data) : fieldOrder;
    const rows = columns.map((fldName, index) => {
        const lbl = formatLabel(fldName);
        const fldVal = data[fldName] ? data[fldName] : '';
        const cmd = data["__" + fldName] ? data["__" + fldName] : null;

        if (cmd) {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, '&quot;')}>{fldVal}</a></td></tr>;
        } else {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td>{fldVal}</td></tr>;
        }

    });
    return <table className="dnn-prompt-tbl"><tbody>{rows}</tbody></table>;
}