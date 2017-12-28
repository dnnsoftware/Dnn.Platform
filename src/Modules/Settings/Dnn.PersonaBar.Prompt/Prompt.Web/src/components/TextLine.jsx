import React from "react";
import Parser from "html-react-parser";

const TextLine = ({txt, css, getKey}) => {
    const textLines = txt.split("\n");
    const rows = textLines.map((line, index) => line ? <span key={getKey("textline")} className={css}>{Parser(line)}</span> : null).reduce((prev,current, index) => {
        if (current !== "" && current !== null && current !== undefined) {
            return [...prev,current];
        }
        return [...prev,<br key={getKey("textline")} />,<br key={getKey("textline")} />];
    }, []);
    return <div key={getKey("textline")}>{rows}</div>;
};

TextLine.defaultProps = { css: "dnn-prompt-ok" };

TextLine.propTypes = {
    txt: React.PropTypes.string.isRequired,
    css: React.PropTypes.string,
    getKey: React.PropTypes.func.isRequired
};

export default TextLine;