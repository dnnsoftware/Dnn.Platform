import React from "react";
import PropTypes from "prop-types";
import Parser from "html-react-parser";
import Localization from "localization/Localization";
import DataTable from "components/DataTable";
import TextLine from "components/TextLine";
import { stripWhiteSpaces } from "utils/helpers";
import DomKey from "services/DomKey";

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
    const out = (<section key={DomKey.get("help")} className="dnn-prompt-inline-help">
        {anchorName}
        {headingName}
        {paragraphDescription}
        {options && options.length > 0 && <h4>{Localization.get("Help_Options")}</h4>}
        {options && options.length > 0 && <div><DataTable rows={options} columns={fields} cssClass="table" /></div>}
        {resultHtml && <div>{Parser(stripWhiteSpaces(resultHtml))}</div>}
    </section>);

    return out;
};

Help.propTypes = {
    IsPaging: PropTypes.func.isRequired,
    style: PropTypes.string.isRequired,
    isError: PropTypes.bool.isRequired,
    error: PropTypes.string,
    name: PropTypes.string.isRequired,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string
};

Help.defaultProps = {
    isError: false
};

export default Help;