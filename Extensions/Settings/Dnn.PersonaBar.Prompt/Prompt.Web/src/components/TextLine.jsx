import React from "react";
import PropTypes from "prop-types";
import Parser from "html-react-parser";
import DomKey from "services/DomKey";

const TextLine = ({txt, css}) => {
    if (!txt) {
        return null;
    }
    const textLines = txt.split("\\n");
    const rows = textLines.map((line) => line ? <span key={DomKey.get("textline")} className={css}>{Parser(line)}</span> : null).reduce((prev,current) => {
        if (current !== "" && current !== null && current !== undefined) {
            return [...prev,current];
        }
        return [...prev,<br key={DomKey.get("textline")} />,<br key={DomKey.get("textline")} />];
    }, []);
    return <div key={DomKey.get("textline")}>{rows}</div>;
};

TextLine.defaultProps = { css: "dnn-prompt-cmd" };

TextLine.propTypes = {
    txt: PropTypes.string.isRequired,
    css: PropTypes.string
};

export default TextLine;