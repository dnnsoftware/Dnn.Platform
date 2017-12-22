import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import "../Prompt.less";
import { sort } from "../../helpers";
import Parser from "html-react-parser";

class Output extends Component {
    constructor() {
        super();
        this.state = {
            commandList: null,
            output: "",
            data: [],
            paging: {},
            isHtml: false,
            isError: false,
            fieldOrder: [],
            reload: false,
            clearOutput: false,
            style: null,
            isHelp: false,
            name: null,
            description: null,
            options: null,
            resultHtml: null,
            error: null,
            nextPageCommand: null
        };
    }

    renderResults() {
        const { props } = this;

        props.IsPaging(false);
        const style = props.style ? props.style : props.isError ? "error" : "ok";
        let { fieldOrder } = props;
        if (props.isHelp) {
            if (props.commandList !== null && props.commandList.length > 0) {
                this.renderCommands();
            } else {
                this.renderHelp();
            }
            return;
        }
        if ((typeof fieldOrder === "undefined" || !fieldOrder || fieldOrder.length === 0) && fieldOrder !== null) {
            fieldOrder = null;
        }
        if (props.clearOutput) {
            if (props.output) {
                this.refs.cmdPromptOutput.innerHTML = "";
                this.writeLine(props.output, style);
            }
            else {
                this.refs.cmdPromptOutput.innerHTML = "";
            }
        }
        else if (props.reload) {
            if (props.output !== null && props.output !== "" && props.output.toLowerCase().indexOf("http") >= 0) {
                window.top.location.href = props.output;
            } else {
                location.reload(true);
            }
        }
        else if (props.data) {
            let html = this.renderData(props.data, fieldOrder);
            this.writeHtml(html);
            if (props.output) { this.writeLine(props.output); }
        }
        else if (props.isHtml) {
            this.writeHtml(props.output);
        }
        else if (props.output) {
            this.writeLine(props.output, style);
        }
        props.busy(false);
        if (props.paging && props.paging.pageNo < props.paging.totalPages && props.nextPageCommand !== null && props.nextPageCommand !== "") {
            props.toggleInput(false);
            props.IsPaging(true);
        }
        props.scrollToBottom();
    }
    renderCommands() {
        let { props } = this;
        props.IsPaging(false);
        let section = document.createElement("section");
        let headingName = document.createElement("h3");
        let paragraphDescription = document.createElement("p");
        let headingCommands = document.createElement("h4");
        let divCommands = document.createElement("div");
        let commandsHtml = "";
        let headingSeeAlso = document.createElement("h4");
        let anchorSyntax = document.createElement("a");
        let anchorLearn = document.createElement("a");
        section.className = "dnn-prompt-inline-help";
        headingName.className = "mono";
        headingName.innerHTML = Localization.get("Prompt_Help_PromptCommands");
        paragraphDescription.className = "lead";
        paragraphDescription.innerHTML = Localization.get("Prompt_Help_ListOfAvailableMsg");
        headingCommands.innerHTML = Localization.get("Prompt_Help_Commands");

        //divCommands.className = "table";
        commandsHtml += "<table class='table'><thead><tr>";
        commandsHtml += `<th>${Localization.get("Prompt_Help_Command")}</th>`;
        commandsHtml += `<th>${Localization.get("Prompt_Help_Description")}</th>`;
        commandsHtml += "</tr></thead>";
        commandsHtml += "<tbody>";
        let currentCategory = "";
        let { commandList } = this.state;
        commandList = sort(Object.assign(commandList), "Category");
        let helpItems = commandList.map((cmd) => {
            let returnHtml = "";
            if (currentCategory !== cmd.Category) {
                currentCategory = cmd.Category;
                returnHtml = `<tr class="divider"><td colspan="2">${currentCategory}</td></tr>`;
            }
            returnHtml += "<tr>";
            returnHtml += "<td class='mono'>";
            returnHtml += `<a class="dnn-prompt-cmd-insert" data-cmd="help ${cmd.Key.toLowerCase()}" href="#">${cmd.Key}</a>`;
            returnHtml += "</td>";
            returnHtml += `<td>${cmd.Description}</td>`;
            returnHtml += "</tr>";
            return returnHtml;
        });
        helpItems.map((line => {
            commandsHtml += line;
        }));
        commandsHtml += "</tbody></table>";
        divCommands.innerHTML = commandsHtml;

        headingSeeAlso.innerHTML = Localization.get("Prompt_Help_SeeAlso");
        anchorSyntax.className = "dnn-prompt-cmd-insert";
        anchorSyntax.setAttribute("data-cmd", "help syntax");
        anchorSyntax.innerHTML = Localization.get("Prompt_Help_Syntax");
        anchorSyntax.href = "#";
        anchorLearn.className = "dnn-prompt-cmd-insert";
        anchorLearn.setAttribute("data-cmd", "help learn");
        anchorLearn.innerHTML = Localization.get("Prompt_Help_Learn");
        anchorLearn.style.marginLeft = "10px";
        anchorLearn.href = "#";

        section.appendChild(headingName);
        section.appendChild(paragraphDescription);
        section.appendChild(headingCommands);
        section.appendChild(divCommands);
        section.appendChild(headingSeeAlso);
        section.appendChild(anchorSyntax);
        section.appendChild(anchorLearn);
        this.refs.cmdPromptOutput.appendChild(section);
        props.scrollToBottom();
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
            <section className="dnn-prompt-inline-help">
                {anchorName}
                {headingName}
                {paragraphDescription}
                {props.options && props.options.length > 0 && <h4>{Localization.get("Help_Options")}</h4>}
                {props.options && props.options.length > 0 && <div>{this.renderTable(props.options, fields, "table")}</div>}
                {props.resultHtml && <div>{Parser(props.resultHtml)}</div>}
            </section>
        );
        // props.scrollToBottom();
        return out;
    }

    writeLine(txt, cssSuffix) {
        const textLines = txt.split("\\n");
        cssSuffix = cssSuffix || "ok";
        return (textLines.map(line => <span className={cssSuffix}>{line}</span>));
    }

    writeHtml(content) {
        return (
            <div>{content}</div>
        );
    }

    renderData(data, fieldOrder) {
        if (data.length > 1) {
            return this.renderTable(data, fieldOrder);
        } else if (data.length === 1) {
            return this.renderObject(data[0], fieldOrder);
        }
        return <br />;
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

    renderTableHeader(columns, cssClass) {
        const tableCols = columns.map(col =>  <th>{this.formatLabel(col)}</th>);
        return (
            <thead>
            <tr>
                {tableCols}
            </tr>
            </thead>
        );
    }

    renderTableRows(rows, columns) {
        return rows.map((row) => {
            return (
                <tr>
                    {columns.map((fieldName) => {
                        let fieldValue = row[fieldName.replace("$", "")] ? row[fieldName.replace("$", "")] : '';
                        let cmd = row["__" + fieldName] ? row["__" + fieldName] : null;
                        if (cmd) {
                            <td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, '&quot;')}>{fieldValue}</a></td>;
                        }
                        else if (fieldName.indexOf("$") >= 0) {
                            <td className="mono">--{fieldValue}</td>;
                        }
                        else {
                            <td>{fieldValue}</td>;
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
            <table className={cssClass ? cssClass : "dnn-prompt-tbl"}>
                {tableHeader}
                <tbody>
                {tableRows}
                </tbody>
            </table>
        );
    }

    renderObject(data, fieldOrder) {

        const columns = !fieldOrder || fieldOrder.length == 0 ? this.getColumnsFromRow(data) : fieldOrder;
        const rows = columns.map((fldName) => {
            const lbl = this.formatLabel(fldName);
            const fldVal = data[fldName] ? data[fldName] : '';
            const cmd = data["__" + fldName] ? data["__" + fldName] : null;

            if (cmd) {
                <tr><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, '&quot;')}>{fldVal}</a></td></tr>;
            } else {
                <tr><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td>{fldVal}</td></tr>;
            }

        });
        return (
            <table className="dnn-prompt-tbl">{rows}</table>
        );
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
        const out = this.renderResults();
        return (
            <div className="dnn-prompt-output">{out}</div>
        );
    }
}
Output.PropTypes = {
    dispatch: PropTypes.func.isRequired,
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
    IsPaging: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        output: state.prompt.output,
        data: state.prompt.data,
        paging: state.prompt.pagingInfo,
        isHtml: state.prompt.isHtml,
        reload: state.prompt.reload,
        style: state.prompt.style,
        fieldOrder: state.prompt.fieldOrder,
        commandList: state.prompt.commandList,
        isError: state.prompt.isError,
        clearOutput: state.prompt.clearOutput,
        isHelp: state.prompt.isHelp,
        name: state.prompt.name,
        description: state.prompt.description,
        options: state.prompt.options,
        resultHtml: state.prompt.resultHtml,
        error: state.prompt.error,
        nextPageCommand: state.prompt.nextPageCommand
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(Output);