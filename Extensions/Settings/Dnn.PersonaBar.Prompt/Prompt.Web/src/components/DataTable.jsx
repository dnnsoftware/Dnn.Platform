import React from "react";
import PropTypes from "prop-types";
import { formatLabel, getColumnsFromRow} from "utils/helpers";
import Parser from "html-react-parser";
import DomKey from "services/DomKey";

const DataTable = ({rows, columns, cssClass}) => {

    const renderTableHeader = (columns) => {
        const tableCols = columns.map((col) => <th key={DomKey.get("datatable")}>{formatLabel(col)}</th>);
        return (
            <thead>
                <tr key={DomKey.get("datatable")}>
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
                    return <td key={DomKey.get("datatable")}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{Parser(fieldValue)}</a></td>;
                }
                else if (fieldName.indexOf("$") >= 0) {
                    return <td key={DomKey.get("datatable")} className="mono">--{Parser(fieldValue)}</td>;
                }
                else {
                    return <td key={DomKey.get("datatable")}>{Parser(fieldValue)}</td>;
                }
            });
            return <tr key={DomKey.get("datatable")}>{rowCells}</tr>;
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
            <table key={DomKey.get("datatable")} className={cssClass ? cssClass : "dnn-prompt-tbl"}>
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
    rows: PropTypes.array.isRequired,
    columns: PropTypes.array.isRequired,
    cssClass: PropTypes.string.isRequired
};

export default DataTable;