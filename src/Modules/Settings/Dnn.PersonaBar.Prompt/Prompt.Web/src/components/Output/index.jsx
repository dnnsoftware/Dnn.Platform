import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import "../Prompt.less";
import { sort } from "../../helpers";

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
    componentWillMount() {
        let { props } = this;
        this.updateState(props);
    }
    shouldComponentUpdate(nextProps, nextState) {
        if (this.shallowCompare(this, nextProps, nextState))
            this.updateState(nextProps);
    }
    shallowEqual(objA, objB) {
        if (objA === objB) {
            return true;
        }

        if (typeof objA !== "object" || objA === null ||
            typeof objB !== "object" || objB === null) {
            return false;
        }

        let keysA = Object.keys(objA);
        let keysB = Object.keys(objB);

        if (keysA.length !== keysB.length) {
            return false;
        }

        // Test for A's keys different from B.
        let bHasOwnProperty = hasOwnProperty.bind(objB);
        for (let i = 0; i < keysA.length; i++) {
            if (typeof objA[keysA[i]] !== "function" || typeof objB[keysA[i]] !== "function") {
                if (!bHasOwnProperty(keysA[i]) || objA[keysA[i]] !== objB[keysA[i]]) {
                    return false;
                }
            }
        }

        return true;
    }
    shallowCompare(instance, nextProps, nextState) {
        let result = (
            !this.shallowEqual(instance.props, nextProps) ||
            !this.shallowEqual(instance.state, nextState)
        );
        return result;
    }

    updateState(props) {
        let { state } = this;
        let self = this;
        state.commandList = props.commandList;
        state.output = props.output;
        state.data = props.data;
        state.paging = props.paging;
        state.isHtml = props.isHtml;
        state.isError = props.isError;
        state.fieldOrder = props.fieldOrder;
        state.reload = props.reload;
        state.clearOutput = props.clearOutput;
        state.style = props.style;
        state.isHelp = props.isHelp;
        state.name = props.name;
        state.description = props.description;
        state.options = props.options;
        state.resultHtml = props.resultHtml;
        state.error = props.error;
        state.nextPageCommand = props.nextPageCommand;
        this.setState({}, () => {
            self.renderResults();
        });
    }
    renderResults() {
        let { state } = this;
        let { props } = this;
        props.IsPaging(false);
        const style = state.style ? state.style : state.isError ? "error" : "ok";
        let { fieldOrder } = state;
        if (state.isHelp) {
            if (state.commandList !== null && state.commandList.length > 0) {
                this.renderCommands();
            } else {
                this.renderHelp();
            }
            return;
        }
        if ((typeof fieldOrder === "undefined" || !fieldOrder || fieldOrder.length === 0) && fieldOrder !== null) {
            fieldOrder = null;
        }
        if (state.clearOutput) {
            if (state.output) {
                this.refs.cmdPromptOutput.innerHTML = "";
                this.writeLine(state.output, style);
            }
            else {
                this.refs.cmdPromptOutput.innerHTML = "";
            }
        }
        else if (state.reload) {
            if (state.output !== null && state.output !== "" && state.output.toLowerCase().indexOf("http") >= 0) {
                window.top.location.href = state.output;
            } else {
                location.reload(true);
            }
        }
        else if (state.data) {
            let html = this.renderData(state.data, fieldOrder);
            this.writeHtml(html);
            if (state.output) { this.writeLine(state.output); }
        }
        else if (state.isHtml) {
            this.writeHtml(state.output);
        }
        else if (state.output) {
            this.writeLine(state.output, style);
        }
        props.busy(false);
        if (state.paging && state.paging.pageNo < state.paging.totalPages && state.nextPageCommand !== null && state.nextPageCommand !== "") {
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
        let { state } = this;
        let { props } = this;
        props.IsPaging(false);
        const style = state.style ? state.style : state.isError ? "error" : "ok";
        if (state.isError) {
            this.writeLine(state.error, style);
            return;
        }
        let section = document.createElement("section");
        let headingName = document.createElement("h3");
        let anchorName = document.createElement("a");
        let paragraphDescription = document.createElement("p");
        section.className = "dnn-prompt-inline-help";
        anchorName.attributes["name"] = state.name;
        headingName.className = "mono";
        headingName.innerHTML = state.name;
        paragraphDescription.className = "lead";
        paragraphDescription.innerHTML = state.description;
        section.appendChild(anchorName);
        section.appendChild(headingName);
        section.appendChild(paragraphDescription);
        //        this.refs.cmdPromptOutput.appendChild(section);
        if (state.options && state.options.length > 0) {
            let headingOptions = document.createElement("h4");
            headingOptions.innerHTML = Localization.get("Help_Options");
            //let fields = ["$" + Localization.get("Help_Flag"), Localization.get("Help_Type"), Localization.get("Help_Required"), Localization.get("Help_Default"), Localization.get("Help_Description")];
            let fields = ["$Flag", "Type", "Required", "Default", "Description"];
            let options = this.renderTable(state.options, fields, "table");
            section.appendChild(headingOptions);
            let div = document.createElement("div");
            div.innerHTML = options;
            section.appendChild(div);
        }
        if (state.resultHtml !== undefined && state.resultHtml !== null && state.resultHtml !== "") {
            let divResults = document.createElement("div");
            divResults.innerHTML = state.resultHtml;
            section.appendChild(divResults);
        }
        this.refs.cmdPromptOutput.appendChild(section);
        props.scrollToBottom();
    }
    newLine() {
        let { props } = this;
        this.refs.cmdPromptOutput.appendChild(document.createElement("br"));
        props.scrollToBottom();
    }

    writeLine(txt, cssSuffix) {
        let textLines = txt.split("\\n");
        textLines.map((line) => {
            let span = document.createElement("span");
            cssSuffix = cssSuffix || "ok";
            span.className = "dnn-prompt-" + cssSuffix;
            span.innerText = line;
            this.refs.cmdPromptOutput.appendChild(span);
            this.newLine();
        });
    }

    writeHtml(markup) {
        let div = document.createElement("div");
        div.innerHTML = markup;
        this.refs.cmdPromptOutput.appendChild(div);
        this.newLine();
    }

    renderData(data, fieldOrder) {
        if (data.length > 1) {
            return this.renderTable(data, fieldOrder);
        } else if (data.length === 1) {
            return this.renderObject(data[0], fieldOrder);
        }
        return "";
    }
    extractLinkFields(row) {
        let linkFields = [];
        if (!row || !row.length) { return linkFields; }

        // find any command link fields
        for (let fld in row) {
            if (fld.startsWith("__")) {
                linkFields.push(fld.slice(2));
            }
        }
        return linkFields;
    }
    renderTable(rows, fieldOrder, cssClass) {
        if (!rows || !rows.length) { return; }
        const linkFields = this.extractLinkFields(rows[0]);

        let columns = fieldOrder;
        if (!columns || !columns.length) {
            // get columns from first row
            columns = [];
            const row = rows[0];
            for (let key in row) {
                if (!key.startsWith("__")) {
                    columns.push(key);
                }
            }
        }

        // build header
        let out = '<table class="' + (cssClass !== undefined && cssClass !== null && cssClass !== '' ? cssClass : "dnn-prompt-tbl") + '"><thead><tr>';
        columns.map((col) => {
            let lbl = this.formatLabel(col);
            out += `<th>${lbl}</th>`;
        });
        out += '</tr></thead><tbody>';

        // build rows
        rows.map((row) => {
            out += '<tr>';
            // only use specified columns
            columns.map((fldName) => {
                //let fldName = columns[fld];
                let fldVal = row[fldName.replace("$", "")] ? row[fldName.replace("$", "")] : '';
                let cmd = row["__" + fldName] ? row["__" + fldName] : null;
                if (cmd) {
                    out += `<td><a href="#" class="dnn-prompt-cmd-insert" data-cmd="${cmd}" title="${cmd.replace(/'/g, '&quot;')}">${fldVal}</a></td>`;
                }
                else if (fldName.indexOf("$") >= 0) {
                    out += `<td class="mono">--${fldVal}</td>`;
                }
                else {
                    out += `<td> ${fldVal}</td>`;
                }
            });
            out += '</tr>';
        });
        out += '</tbody></table>';
        return out;
    }

    renderObject(data, fieldOrder) {
        const linkFields = this.extractLinkFields(data);
        let columns = fieldOrder;
        if (!columns || !columns.length) {
            // no field order. Generate it
            columns = [];
            for (let key in data) {
                if (!key.startsWith("__")) {
                    columns.push(key);
                }
            }
        }
        let out = '<table class="dnn-prompt-tbl">';
        columns.map((fldName) => {
            let lbl = this.formatLabel(fldName);
            let fldVal = data[fldName] ? data[fldName] : '';
            let cmd = data["__" + fldName] ? data["__" + fldName] : null;

            if (cmd) {
                out += `<tr><td class="dnn-prompt-lbl">${lbl}</td><td>:</td><td><a href="#" class="dnn-prompt-cmd-insert" data-cmd="${cmd}" title="${cmd.replace(/'/g, '&quot;')}">${fldVal}</a></td></tr>`;
            } else {
                out += `<tr><td class="dnn-prompt-lbl">${lbl}</td><td>:</td><td>${fldVal}</td></tr>`;
            }

        });
        out += '</table>';
        return out;
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
        return (
            <div className="dnn-prompt-output" ref="cmdPromptOutput">
            </div>
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