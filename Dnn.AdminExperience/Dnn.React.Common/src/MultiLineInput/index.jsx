import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";

const getOpts = function (enabled) {
    let opts = {};
    if (!enabled) {
        opts["disabled"] = "disabled";
    }
    return opts;
};

const getHandler = function (handler, enabled) {
    return handler && enabled ? handler : null;
};

class MultiLineInput extends Component {
    constructor() {
        super();
    }

    render() {
        const {props} = this;
        return (
            <textarea className={"dnn-ui-common-multi-line-input" + (" " + props.className)}
                id={props.id}
                onChange={getHandler(props.onChange, props.enabled) }
                onBlur={getHandler(props.onBlur, props.enabled) }
                onFocus={getHandler(props.onFocus, props.enabled) }
                onKeyDown={getHandler(props.onKeyDown, props.enabled) }
                onKeyPress={getHandler(props.onKeyPress, props.enabled) }
                onKeyUp={getHandler(props.onKeyUp, props.enabled) }
                value={props.value}
                tabIndex={props.tabIndex}
                style={props.style}
                placeholder={props.placeholder}
                maxLength={props.maxLength}
                aria-label="Content"
                ref={props.inputRef}
                {...getOpts(props.enabled) }/>
        );
    }
}

MultiLineInput.propTypes = {
    id: PropTypes.string,
    className: PropTypes.string,
    onChange: PropTypes.func,
    onBlur: PropTypes.func,
    onFocus: PropTypes.func,
    onKeyDown: PropTypes.func,
    onKeyPress: PropTypes.func,
    onKeyUp: PropTypes.func,
    value: PropTypes.string,
    enabled: PropTypes.bool,
    tabIndex: PropTypes.number,
    placeholder: PropTypes.string,
    style: PropTypes.object,
    maxLength: PropTypes.number
};

MultiLineInput.defaultProps = {
    className: "", //prevents undefined
    enabled: true
};

export default MultiLineInput;