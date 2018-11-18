import React from "react";
import PropTypes from "prop-types";
import Parser from "html-react-parser";
import Localization from "localization/Localization";
//import { sort } from "utils/helpers";
import DomKey from "services/DomKey";

const Command = ({commandList, IsPaging}) => {

    IsPaging(false);

    const headingName = <h3 className="mono">{Parser(Localization.get("Prompt_Help_PromptCommands"))}</h3>;
    const paragraphDescription = <p className="lead">{Parser(Localization.get("Prompt_Help_ListOfAvailableMsg"))}</p>;
    const headingCommands = <h4>{Parser(Localization.get("Prompt_Help_Commands"))}</h4>;

    const commandsList = commandList.sort((a,b) => {
        const catA = a.Category;
        const catB = b.Category;
        const kA = a.Key;
        const kB = b.Key;

        if (catA === catB && kA === kB) return 0;

        if (catA === catB) {
            return kA < kB ? -1 : 1;
        } else {
            return catA < catB ? -1 : 1;
        }

    }).reduce((prev,current,index, arr) => {
        if (index > 0) {
            const currentCat = current.Category;
            const prevCat = arr[index - 1].Category;
            if (currentCat !== prevCat) {
                return [...prev, {separator: true, Category: current.Category}, current];
            }
        } else {
            return [{separator: true, Category: current.Category}, current];
        }

        return [...prev, current];

    }, []);

    const commandsOutput = commandsList.map((cmd) => {
        if (cmd.separator) {
            return <tr key={DomKey.get("command")} className="divider"><td colSpan="2">{cmd.Category}</td></tr>;
        }

        return (
            <tr key={DomKey.get("command")}>
                <td key={DomKey.get("command")} className="mono"><a className="dnn-prompt-cmd-insert" data-cmd={`help ${cmd.Key.toLowerCase()}`} href="#">{cmd.Key}</a></td>
                <td key={DomKey.get("command")}>{Parser(cmd.Description)}</td>
            </tr>
        );
    });
    const divCommands = (<div>
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
    </div>);

    const headingSeeAlso = <h4>{Parser(Localization.get("Prompt_Help_SeeAlso"))}</h4>;
    const anchorSyntax = <a href="#" className="dnn-prompt-cmd-insert" data-cmd="help syntax">{Parser(Localization.get("Prompt_Help_Syntax"))}</a>;
    const anchorLearn = <a href="#" className="dnn-prompt-cmd-insert" style={{marginLeft:"10px"}} data-cmd="help learn">{Parser(Localization.get("Prompt_Help_Learn"))}</a>;

    const out = (
        <section className="dnn-prompt-inline-help">
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
};

Command.propTypes = {
    commandList: PropTypes.array.isRequired,
    IsPaging: PropTypes.func.isRequired
};

export default Command;