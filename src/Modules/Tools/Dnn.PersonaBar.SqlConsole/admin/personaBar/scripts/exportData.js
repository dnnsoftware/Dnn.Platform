define([], function () {
    "use strict";
    var template = { excel: '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>' };
    var csvDelimiter = ",";
    var csvNewLine = "\r\n";
    var format = function (s, c) {
        return s.replace(new RegExp("{(\\w+)}", "g"), function (m, p) {
            return c[p];
        });
    };

    var fixCsvField = function (value) {
        var fixedValue = value;
        var addQuotes = (value.indexOf(csvDelimiter) !== -1) || (value.indexOf('\r') !== -1) || (value.indexOf('\n') !== -1);
        var replaceDoubleQuotes = (value.indexOf('"') !== -1);

        if (replaceDoubleQuotes) {
            fixedValue = fixedValue.replace(/"/g, '""');
        }
        if (addQuotes || replaceDoubleQuotes) {
            fixedValue = '"' + fixedValue + '"';
        }
        return fixedValue;
    };

    var tableToCsv = function (table) {
        var data = "";
        var i, j, row, col;
        for (i = 0; i < table.rows.length; i++) {
            row = table.rows[i];
            for (j = 0; j < row.cells.length; j++) {
                col = row.cells[j];
                data = data + (j ? csvDelimiter : '') + fixCsvField(col.textContent.trim());
            }
            data = data + csvNewLine;
        }
        return data;
    };

    return {
        excel: function (table, name) {
            var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML };
            return format(template.excel, ctx);
        },
        csv: function (table, delimiter, newLine) {
            if (delimiter !== undefined && delimiter) {
                csvDelimiter = delimiter;
            }
            if (newLine !== undefined && newLine) {
                csvNewLine = newLine;
            }
            return tableToCsv(table);
        }
    };
}());