import React from "react";
import { formatLabel} from "./util";

const DataTable = ({rows, columns, cssClass}) => {

    const renderTableHeader = (columns) => {
        const tableCols = columns.map((col,index) =>  <th key={index}>{formatLabel(col)}</th>);
        return (
            <thead>
            <tr>
                {tableCols}
            </tr>
            </thead>
        );
    };

    const renderTableRows = (rows, columns) => {
        return rows.map((row, index) => {
            return (
                <tr key={index}>
                    {columns.map((fieldName, index) => {
                        let fieldValue = row[fieldName.replace("$", "")] ? row[fieldName.replace("$", "")] : "";
                        let cmd = row["__" + fieldName] ? row["__" + fieldName] : null;
                        if (cmd) {
                            return <td key={`table-${index}`}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{fieldValue}</a></td>;
                        }
                        else if (fieldName.indexOf("$") >= 0) {
                            return <td key={`table-${index}`} className="mono">--{fieldValue}</td>;
                        }
                        else {
                            return <td key={`table-${index}`}>{fieldValue}</td>;
                        }
                    })}
                </tr>
            );
        });
    };

    const renderTable = () => {
        if (!rows || !rows.length) { return; }

        // build header
        const tableHeader = renderTableHeader(columns, cssClass);

        // build rows
        const tableRows = renderTableRows(rows, columns);

        return (
            <table className={cssClass ? cssClass : "dnn-prompt-tbl"}>
                {tableHeader}
                <tbody>
                {tableRows}
                </tbody>
            </table>
        );
    };

    return renderTable();
};

DataTable.propTypes = {
    rows: React.PropTypes.array.isRequired,
    columns: React.PropTypes.array.isRequired,
    cssClass: React.PropTypes.string.isRequired
};

export default DataTable;