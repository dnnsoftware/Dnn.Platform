import React, { Component, PropTypes } from "react";
import Localization from "localization";
import "../Prompt.less";
import { sort } from "../../helpers";
import Parser from "html-react-parser";

class Output extends Component {

    renderResults() {
        const { props } = this;

        props.IsPaging(false);
        let { fieldOrder } = props;
        if (props.isHelp) {
            if (props.commandList !== null && props.commandList.length > 0) {
                return this.renderCommands();
            } else {
                return this.renderHelp();
            }
        }
        if ((typeof fieldOrder === "undefined" || !fieldOrder || fieldOrder.length === 0) && fieldOrder !== null) {
            fieldOrder = null;
        }
        else if (props.reload) {
            if (props.output !== null && props.output !== "" && props.output.toLowerCase().indexOf("http") >= 0) {
                window.top.location.href = props.output;
            } else {
                location.reload(true);
            }
        }
        else if (props.data) {
            return this.renderData(props.data, fieldOrder);
        }
        else if (props.isHtml) {
            return this.writeHtml(props.output);
        }
        else if (props.output) {
            const style = props.isError ? "error" : "ok dnn-prompt-input";
            return this.writeLine(props.output, style);
        }

        props.busy(false);
        if (props.paging && props.paging.pageNo < props.paging.totalPages && props.nextPageCommand !== null && props.nextPageCommand !== "") {
            props.toggleInput(false);
            props.IsPaging(true);
        }
    }

    renderCommands() {
        const { props } = this;
        props.IsPaging(false);

        const headingName = <h3 className="mono">{Parser(Localization.get("Prompt_Help_PromptCommands"))}</h3>;
        const paragraphDescription = <p className="lead">{Parser(Localization.get("Prompt_Help_ListOfAvailableMsg"))}</p>;
        const headingCommands = <h4>{Parser(Localization.get("Prompt_Help_Commands"))}</h4>;

        const commandList = sort(Object.assign(props.commandList), "Category").reduce((prev,current,index, arr) => {
            if(index > 0) {
                const currentCat = current.Category;
                const prevCat = arr[index - 1].Category;
                if(currentCat != prevCat)
                    return [...prev, {separator: true, Category: current.Category}, current];
            }

            return [...prev, current];

        }, []);

        const commandsOutput = commandList.map((cmd, index) => {
            if (cmd.separator) {
                return <tr key={index} className="divider"><td colSpan="2">{cmd.Category}</td></tr>;
            }

            return (
                <tr key={index}>
                    <td key={this.getKey("cmdtd")} className="mono"><a className="dnn-prompt-cmd-insert" data-cmd="help {cmd.Key.toLowerCase()}" href="#">{cmd.Key}</a></td>
                    <td key={this.getKey("cmdtd")}>{cmd.Description}</td>
                </tr>
            );
        });
        const divCommands = (
            <div>
                <table className="table">
                    <thead>
                        <tr>
                            <th>{Parser(Localization.get("Prompt_Help_Command"))}</th>
                            <th>{Parser(Localization.get("Prompt_Help_Description"))}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {commandsOutput}
                    </tbody>
                </table>
            </div>
        );

        const headingSeeAlso = <h4>{Parser(Localization.get("Prompt_Help_SeeAlso"))}</h4>;
        const anchorSyntax = <a href="#" className="dnn-prompt-cmd-insert" data-cmd="help syntax">{Parser(Localization.get("Prompt_Help_Syntax"))}</a>;
        const anchorLearn = <a href="#" className="dnn-prompt-cmd-insert" style={{marginLeft:"10px"}} data-cmd="help learn">{Parser(Localization.get("Prompt_Help_Learn"))}</a>;

        const out = (
            <section key={this.getKey("command")} className="dnn-prompt-inline-help">
                {headingName}
                {paragraphDescription}
                {headingCommands}
                {divCommands}
                {headingSeeAlso}
                {anchorSyntax}
                {anchorLearn}
            </section>
        );

        return out;
    }

    getKey(prefix) {
        if(this.key === undefined) {
            this.key = 0;
        }
        return prefix ? `${prefix}-${this.key++}` : this.key++;
    }

    renderHelp() {
        const { props } = this;
        props.IsPaging(false);
        const style = props.style ? props.style : props.isError ? "error" : "ok";
        if (props.isError) {
            return this.writeLine(props.error, style);
        }

        const headingName = <h3 className="mono">{props.name}</h3>;
        const anchorName = <a name={props.name}></a>;
        const paragraphDescription = <p className="lead">{props.description}</p>;
        const fields = ["$Flag", "Type", "Required", "Default", "Description"];
        const out = (
            <section key={this.getKey("help")} className="dnn-prompt-inline-help">
                {anchorName}
                {headingName}
                {paragraphDescription}
                {props.options && props.options.length > 0 && <h4>{Localization.get("Help_Options")}</h4>}
                {props.options && props.options.length > 0 && <div>{this.renderTable(props.options, fields, "table")}</div>}
                {props.resultHtml && <div>{Parser(props.resultHtml)}</div>}
            </section>
        );
        return out;
    }

    writeLine(txt, cssSuffix) {
        const textLines = txt.split("\n");
        cssSuffix = cssSuffix || "ok";
        const rows = textLines.map((line, index) => line ? <span key={index} className={cssSuffix}>{Parser(line)}</span> : null).reduce((prev,current) => {
            if(current != "" && current != null && current != undefined) {
                return [...prev,current];
            }
            return [...prev,<br key={this.getKey("line")}/>,<br key={this.getKey("line")}/>];
        }, []);
        return <div key={this.getKey("line")}>{rows}</div>;
    }

    writeHtml(content) {
        return <div key={this.getKey("html")}>{Parser(content)}</div>;
    }

    renderData(data, fieldOrder) {
        if (data.length > 1) {
            return this.renderTable(data, fieldOrder);
        } else if (data.length === 1) {
            return this.renderObject(data[0], fieldOrder);
        }
        return <br  key={this.getKey("data")} />;
    }

    getColumnsFromRow(row) {
        const columns = [];
        for (let key in row) {
            if (!key.startsWith("__")) {
                columns.push(key);
            }
        }
        return columns;
    }

    renderTableHeader(columns) {
        const tableCols = columns.map((col,index) =>  <th key={index}>{this.formatLabel(col)}</th>);
        return (
            <thead>
            <tr>
                {tableCols}
            </tr>
            </thead>
        );
    }

    renderTableRows(rows, columns) {
        return rows.map((row, index) => {
            return (
                <tr key={index}>
                    {columns.map((fieldName) => {
                        let fieldValue = row[fieldName.replace("$", "")] ? row[fieldName.replace("$", "")] : '';
                        let cmd = row["__" + fieldName] ? row["__" + fieldName] : null;
                        if (cmd) {
                            return <td key={this.getKey("table")}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, '&quot;')}>{fieldValue}</a></td>;
                        }
                        else if (fieldName.indexOf("$") >= 0) {
                            return <td key={this.getKey("table")} className="mono">--{fieldValue}</td>;
                        }
                        else {
                            return <td key={this.getKey("table")}>{fieldValue}</td>;
                        }
                    })}
                </tr>
            );
        });
    }

    renderTable(rows, fieldOrder, cssClass) {
        if (!rows || !rows.length) { return; }
        const firstRow = rows[0];

        const columns = !fieldOrder || fieldOrder.length == 0 ? this.getColumnsFromRow(firstRow) : fieldOrder;

        // build header
        const tableHeader = this.renderTableHeader(columns, cssClass);

        // build rows
        const tableRows = this.renderTableRows(rows, columns);

        return (
            <table key={this.getKey("table")} className={cssClass ? cssClass : "dnn-prompt-tbl"}>
                {tableHeader}
                <tbody>
                {tableRows}
                </tbody>
            </table>
        );
    }

    renderObject(data, fieldOrder) {

        const columns = !fieldOrder || fieldOrder.length == 0 ? this.getColumnsFromRow(data) : fieldOrder;
        const rows = columns.map((fldName, index) => {
            const lbl = this.formatLabel(fldName);
            const fldVal = data[fldName] ? data[fldName] : '';
            const cmd = data["__" + fldName] ? data["__" + fldName] : null;

            if (cmd) {
                return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, '&quot;')}>{fldVal}</a></td></tr>;
            } else {
                return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td>{fldVal}</td></tr>;
            }

        });
        return <table key={this.getKey("object")} className="dnn-prompt-tbl"><tbody>{rows}</tbody></table>;
    }

    formatLabel(input) {
        if (typeof input === "string") {
            // format camelcase and remove Is from labels
            let output = input.replace("$", "").replace(/^(Is)(.+)/i, "$2");
            output = output.match(/[A-Z][a-z]+/g).join(" "); // rudimentary but should handle normal Camelcase
            return output;
        }
        return "";
    }

    render() {
        const { props } = this;
        this.output = !props.clearOutput && this.output ? this.output : [];
        const out = this.renderResults();
        this.output.push(out);
        return (
            <div className="dnn-prompt-output">
                {!props.clearOutput && this.output}
            </div>
        );
    }
}
Output.PropTypes = {
    output: PropTypes.string,
    data: PropTypes.array,
    paging: PropTypes.object,
    isHtml: PropTypes.bool,
    reload: PropTypes.bool,
    isError: PropTypes.bool,
    clearOutput: PropTypes.bool,
    fieldOrder: PropTypes.array,
    commandList: PropTypes.array,
    style: PropTypes.string,
    isHelp: PropTypes.bool,
    name: PropTypes.string,
    nextPageCommand: PropTypes.string,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string,
    error: PropTypes.string,
    scrollToBottom: PropTypes.func.isRequired,
    busy: PropTypes.func.isRequired,
    toggleInput: PropTypes.func.isRequired,
    IsPaging: PropTypes.func.isRequired,
    updateHistory: PropTypes.func.isRequired,
    setHeight: PropTypes.func.isRequired
};

export default Output;