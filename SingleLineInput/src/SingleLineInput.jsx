import React, {PropTypes, Component} from "react";
import "./style.less";

const getOptions = function (enabled) {
    let opts = {};
    if (!enabled) {
        opts["disabled"] = "disabled";
    }
    return opts;
};

const getHandler = function (handler, enabled) {
    return handler && enabled ? handler : null;
};

const getType = function (type) {
    return type ? type : "text";
};
class SingleLineInput extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;
        return (
            <input type={getType(props.type)}
                   className={"dnn-ui-common-single-line-input" + (" " + props.className) + (" " + props.size)}
                   id={props.id}
                   onChange={getHandler(props.onChange, props.enabled)}
                   onBlur={getHandler(props.onBlur, props.enabled)}
                   onFocus={getHandler(props.onFocus, props.enabled)}
                   onKeyDown={getHandler(props.onKeyDown, props.enabled)}
                   onKeyPress={getHandler(props.onKeyPress, props.enabled)}
                   onKeyUp={getHandler(props.onKeyUp, props.enabled)}
                   value={props.value}
                   tabIndex={props.tabIndex}
                   style={props.style} 
                   placeholder={props.placeholder}
                   autoComplete={props.autoComplete}
                   maxLength={props.maxLength}
                   aria-label="Content"
                   {...getOptions(props.enabled)} />
        );
    } 
}

SingleLineInput.propTypes = {
    id: PropTypes.string,
    onChange: PropTypes.func,
    onBlur: PropTypes.func,
    onFocus: PropTypes.func,
    onKeyDown: PropTypes.func,
    onKeyPress: PropTypes.func,
    onKeyUp: PropTypes.func,
    value: PropTypes.any,    
    enabled: PropTypes.bool,
    tabIndex: PropTypes.number,
    style: PropTypes.object,
    placeholder: PropTypes.string,
    type: PropTypes.string,
    size: PropTypes.oneOf(["large", "small"]),
    className: PropTypes.string,
    autoComplete: PropTypes.oneOf(["off", "on"]),
    maxLength: PropTypes.number
};

SingleLineInput.defaultProps = {
    className: "", //prevents undefined
    enabled: true,
    size: "small",
    autoComplete: "on"
};

export default SingleLineInput;
