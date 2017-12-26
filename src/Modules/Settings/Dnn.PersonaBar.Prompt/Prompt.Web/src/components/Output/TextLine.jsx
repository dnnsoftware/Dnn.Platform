import React from "react";
import Parser from "html-react-parser";

const TextLine = ({txt, css}) => {
    const textLines = txt.split("\n");
    const rows = textLines.map((line, index) => line ? <span key={index} className={css}>{Parser(line)}</span> : null).reduce((prev,current, index) => {
        if (current !== "" && current !== null && current !== undefined) {
            return [...prev,current];
        }
        return [...prev,<br key={`textline-${index}`}/>,<br key={`textline-${index}`}/>];
    }, []);
    return <div>{rows}</div>;
};

TextLine.defaultProps = { css: "dnn-prompt-ok" };

TextLine.propTypes = {
    txt: React.PropTypes.string.isRequired,
    css: React.PropTypes.string
};

export default TextLine;