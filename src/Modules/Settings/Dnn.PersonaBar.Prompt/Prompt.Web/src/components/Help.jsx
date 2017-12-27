import React from "react";
import Parser from "html-react-parser";
import Localization from "localization/Localization";
import DataTable from "./DataTable";
import TextLine from "./TextLine";

const Help = ({ style, isError, error, name, description, options, IsPaging, resultHtml }) => {

    IsPaging(false);

    const css = style ? style : isError ? "dnn-prompt-error" : "dnn-prompt-ok";
    if (isError) {
        return <TextLine txt={error} css={css}/>;
    }

    const headingName = <h3 className="mono">{name}</h3>;
    const anchorName = <a name={name}></a>;
    const paragraphDescription = <p className="lead">{description}</p>;
    const fields = ["$Flag", "Type", "Required", "Default", "Description"];
    const out = (
        <section className="dnn-prompt-inline-help">
            {anchorName}
            {headingName}
            {paragraphDescription}
            {options && options.length > 0 && <h4>{Localization.get("Help_Options")}</h4>}
            {options && options.length > 0 && <div><DataTable rows={options} columns={fields} cssClass="table" /></div>}
            {resultHtml && <div>{Parser(resultHtml)}</div>}
        </section>
    );

    return out;
};

Help.propTypes = {
    IsPaging: React.PropTypes.func.isRequired,
    style: React.PropTypes.string.isRequired,
    isError: React.PropTypes.bool.isRequired,
    error: React.PropTypes.string,
    name: React.PropTypes.string.isRequired,
    description: React.PropTypes.description,
    options: React.PropTypes.array,
    resultHtml: React.PropTypes.string
};

Help.defaultProps = {
    isError: false
};

export default Help;