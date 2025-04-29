import React from "react";
import PropTypes from "prop-types";
import Html from "./Html";
import DomKey from "../services/DomKey";

const TextLine = ({ txt, css, isHtml }) => {
  if (!txt) {
    return null;
  }
  const textLines = txt.split("\\n");
  const rows = textLines
    .map((line) =>
      line ? (
        <span key={DomKey.get("textline")} className={css}>
          {isHtml ? <Html html={line} /> : line}
        </span>
      ) : null,
    )
    .reduce((prev, current) => {
      if (current !== "" && current !== null && current !== undefined) {
        return [...prev, current];
      }
      return [
        ...prev,
        <br key={DomKey.get("textline")} />,
        <br key={DomKey.get("textline")} />,
      ];
    }, []);
  return <div key={DomKey.get("textline")}>{rows}</div>;
};

TextLine.defaultProps = { css: "dnn-prompt-cmd" };

TextLine.propTypes = {
  txt: PropTypes.string.isRequired,
  css: PropTypes.string,
  isHtml: PropTypes.bool,
};

export default TextLine;
