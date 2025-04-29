import React from "react";
import PropTypes from "prop-types";
import {
  formatLabel,
  getColumnsFromRow,
  renderObjectFieldCell,
} from "../utils/helpers";
import DomKey from "../services/DomKey";

const DataTable = ({ rows, columns, cssClass }) => {
  const renderTableHeader = (columns) => {
    const tableCols = columns.map((col) => (
      <th key={DomKey.get("datatable")}>{formatLabel(col)}</th>
    ));
    return (
      <thead>
        <tr key={DomKey.get("datatable")}>{tableCols}</tr>
      </thead>
    );
  };

  const renderTableRows = (rows, columns) => {
    return rows.map((row) => (
      <tr key={DomKey.get("datatable")}>
        {columns.map((fieldName) =>
          renderObjectFieldCell(row, fieldName, DomKey.get("datatable")),
        )}
      </tr>
    ));
  };

  const renderTable = (columns) => {
    if (!rows || !rows.length) {
      return;
    }

    columns = columns ? columns : getColumnsFromRow(rows[0]);

    return (
      <table
        key={DomKey.get("datatable")}
        className={cssClass ? cssClass : "dnn-prompt-tbl"}
      >
        {renderTableHeader(columns, cssClass)}
        <tbody>{renderTableRows(rows, columns)}</tbody>
      </table>
    );
  };

  return renderTable(columns);
};

DataTable.propTypes = {
  rows: PropTypes.array.isRequired,
  columns: PropTypes.array.isRequired,
  cssClass: PropTypes.string.isRequired,
};

export default DataTable;
