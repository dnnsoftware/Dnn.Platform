import React, {Component, PropTypes } from "react";
import Tooltip from "dnn-tooltip";
import TextArea from "dnn-multi-line-input";
import Label from "dnn-label";
import "./style.less";

class MultiLineInputWithError extends Component {
    constructor() {
        super();
    }

    render() {
        const {props} = this;
        const className = "dnn-multi-line-input-with-error" + (props.error ? " error" : "") + (" " + props.className) + (props.enabled ? "" : " disabled");
        const errorMessages = props.errorMessage instanceof Array ? props.errorMessage : [props.errorMessage];
        return (
            <div className={className} style={props.style}>
                {props.label &&
                    <Label
                        labelFor={props.inputId}
                        label={props.label}
                        tooltipMessage={props.tooltipMessage}
                        tooltipPlace={props.infoTooltipPlace}
                        tooltipActive={props.tooltipMessage}
                        labelType={props.labelType}
                        className={props.infoTooltipClassName}
                        style={Object.assign(!props.tooltipMessage ? { marginBottom: 5 } : {}, props.labelStyle) }
                        />
                }
                {props.extraToolTips}
                <div className={"input-tooltip-container " + props.labelType}>
                    <TextArea
                        id={props.inputId}
                        onChange={props.onChange}
                        onBlur={props.onBlur}
                        onFocus={props.onFocus}
                        onKeyDown={props.onKeyDown}
                        onKeyPress={props.onKeyPress}
                        onKeyUp={props.onKeyUp}
                        value={props.value}
                        tabIndex={props.tabIndex}
                        style={Object.assign({ marginBottom: 32 }, props.inputStyle) }
                        placeholder={props.placeholder}
                        enabled={props.enabled}
                        maxLength={props.maxLength}
                        />
                    <Tooltip
                        messages={errorMessages}
                        type="error"
                        className={props.placement}
                        tooltipPlace={props.tooltipPlace}
                        rendered={props.error}/>
                </div>
            </div>
        );
    }
}

MultiLineInputWithError.propTypes = {
    inputId: PropTypes.string,
    label: PropTypes.string,
    infoTooltipClassName: PropTypes.string,
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    infoTooltipPlace: PropTypes.string,
    labelType: PropTypes.string,
    className: PropTypes.string,
    inputSize: PropTypes.oneOf(["large", "small"]),
    error: PropTypes.bool,
    errorMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    tooltipPlace: PropTypes.string,
    placement: PropTypes.oneOf(["outside", "inside"]),
    onChange: PropTypes.func,
    onBlur: PropTypes.func,
    onFocus: PropTypes.func,
    onKeyDown: PropTypes.func,
    onKeyPress: PropTypes.func,
    onKeyUp: PropTypes.func,
    value: PropTypes.any,
    enabled: PropTypes.bool,
    tabIndex: PropTypes.number,
    inputStyle: PropTypes.object,
    placeholder: PropTypes.string,
    style: PropTypes.object,
    labelStyle: PropTypes.object,
    extraToolTips: PropTypes.node,
    maxLength: PropTypes.number
};
MultiLineInputWithError.defaultProps = {
    error: false,
    enabled: true,
    className: "",
    placement: "inside",
    labelType: "block",
    errorMessage: ["This field has an error."]
};
export default MultiLineInputWithError;