import React from "react";
import { formatLabel, getColumnsFromRow} from "utils/helpers";
import Parser from "html-react-parser";

const DataTable = ({rows, columns, cssClass, getKey}) => {

    const renderTableHeader = (columns) => {
        const tableCols = columns.map((col) => <th key={getKey("datatable")}>{formatLabel(col)}</th>);
        return (
            <thead>
                <tr key={getKey("datatable")}>
                    {tableCols}
                </tr>
            </thead>
        );
    };

    const renderTableRows = (rows, columns) => {
        return rows.map((row) => {
            const rowCells = columns.map((fieldName) => {
                let fieldValue = row[fieldName.replace("$", "")] ? row[fieldName.replace("$", "")] + "" : "";
                let cmd = row["__" + fieldName] ? row["__" + fieldName] : null;
                if (cmd) {
                    return <td key={getKey("datatable")}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{Parser(fieldValue)}</a></td>;
                }
                else if (fieldName.indexOf("$") >= 0) {
                    return <td key={getKey("datatable")} className="mono">--{Parser(fieldValue)}</td>;
                }
                else {
                    return <td key={getKey("datatable")}>{Parser(fieldValue)}</td>;
                }
            });
            return <tr key={getKey("datatable")}>{rowCells}</tr>;
        });
    };

    const renderTable = (columns) => {
        if (!rows || !rows.length) { return; }

        columns = columns ? columns : getColumnsFromRow(rows[0]);

        // build header
        const tableHeader = renderTableHeader(columns, cssClass);

        // build rows
        const tableRows = renderTableRows(rows, columns);

        return (
            <table key={getKey("datatable")} className={cssClass ? cssClass : "dnn-prompt-tbl"}>
                {tableHeader}
                <tbody>
                {tableRows}
                </tbody>
            </table>
        );
    };

    return renderTable(columns);
};

DataTable.propTypes = {
    rows: React.PropTypes.array.isRequired,
    columns: React.PropTypes.array.isRequired,
    cssClass: React.PropTypes.string.isRequired,
    getKey: React.PropTypes.func.isRequired
};

export default DataTable;