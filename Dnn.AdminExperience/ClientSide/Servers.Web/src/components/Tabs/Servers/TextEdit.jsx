import React from "react";
import PropTypes from "prop-types";
import {
  SingleLineInputWithError,
  IconButton,
} from "@dnnsoftware/dnn-react-common";

function TextEdit(props) {
  if (props.inEdit) {
    return (
      <SingleLineInputWithError
        value={props.text}
        type="Text"
        onChange={(e) => props.onChange(e.target.value)}
        error={!!props.error}
        errorMessage={props.error}
      />
    );
  } else {
    return (
      <div>
        <span>{props.text}</span>
        <IconButton
          type="edit"
          width="16"
          height="16"
          onClick={props.toggleEdit.bind(this)}
        />
      </div>
    );
  }
}

export default TextEdit;

TextEdit.propTypes = {
  text: PropTypes.string,
  inEdit: PropTypes.bool,
  toggleEdit: PropTypes.func,
  onChange: PropTypes.func,
  error: PropTypes.string,
};
