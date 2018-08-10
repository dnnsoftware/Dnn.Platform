import React, { Component, PropTypes } from "react";
import Localization from "../../localization";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";

class KeyWords extends Component {

    render() {
        const {props} = this;
        return <div className="input-group">
            <MultiLineInputWithError
                className="keywords-field"
                label={Localization.get("Keywords")}
                value={props.keywords}
                onChange={(value) => props.onChangeEvent("keywords", value)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

KeyWords.propTypes = {
    keywords: PropTypes.string.isRequired,
    onChangeEvent: PropTypes.func.isRequired
};

export default KeyWords;