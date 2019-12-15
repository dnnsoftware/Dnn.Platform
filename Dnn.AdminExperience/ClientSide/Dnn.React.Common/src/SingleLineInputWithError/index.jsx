import React, { Component } from "react";
import PropTypes from "prop-types";
import Tooltip from "../Tooltip";
import Input from "../SingleLineInput";
import Label from "../Label";
import "./style.less";

class SingleLineInputWithError extends Component {
    constructor() {
        super();
        this.state = {
            isFocused: false
        };
    }

    getClass() {
        const {props} = this;
        const errorClass = props.error ? " " + props.errorSeverity : "";
        const enabledClass = props.enabled ? "" : " disabled";
        const customClass = " " + props.className;
        return "dnn-single-line-input-with-error" + errorClass + customClass + enabledClass;
    }

    onBlur(e) {
        const {props} = this;
        if (props.hasOwnProperty("onChange")) {
            props.onChange(e);
        }
        if (props.hasOwnProperty("onBlur")) {
            props.onBlur(e);
        }
        this.setState({isFocused: false});
    }

    onFocus(e) {
        const {props} = this;
        if (props.hasOwnProperty("onFocus")) {
            props.onFocus(e);
        }
        this.setState({isFocused: true});
    }

    getCounter(counter) {
        if (!this.shouldRenderCounter(counter)) {
            return null;
        }

        return (
            <div className="dnn-inline-counter">
                {counter}
            </div>
        );
    }

    shouldRenderCounter(counter) {
        const counterIsDefined = !!counter || counter === 0;
        return this.state.isFocused && counterIsDefined;
    }

    getInputRightPadding(counter, error) {
        let padding = 0;
        if (counter || counter === 0) {
            padding += 10 + counter.toString().length * 8;
        }
        if (error) {
            padding += 22;
        }

        return padding;
    }

    render() {
        const {props} = this;
        const errorMessages = props.errorMessage instanceof Array ? props.errorMessage : [props.errorMessage];

        return (
            <div className={this.getClass()} style={props.style}>
                {props.label &&
                    <Label
                        labelFor={props.inputId}
                        label={props.label}
                        tooltipMessage={props.tooltipMessage}
                        tooltipPlace={props.infoTooltipPlace}
                        tooltipActive={props.tooltipMessage}
                        labelType={props.labelType}
                        className={props.infoTooltipClassName}
                        style={Object.assign(!props.tooltipMessage ? { marginBottom: 5 } : {}, props.labelStyle)}
                    />
                }
                {props.extraToolTips}
                <div className={"input-tooltip-container " + props.labelType}>
                    <Input
                        id={props.inputId}
                        type={props.type}
                        onChange={props.onChange}
                        onBlur={this.onBlur.bind(this)}
                        onFocus={this.onFocus.bind(this)}
                        onKeyDown={props.onKeyDown}
                        onKeyPress={props.onKeyPress}
                        onKeyUp={props.onKeyUp}
                        value={props.value}
                        tabIndex={props.tabIndex}
                        style={Object.assign({ marginBottom: 32, paddingRight: this.getInputRightPadding(props.counter, props.error)}, props.inputStyle)}
                        placeholder={props.placeholder}
                        enabled={props.enabled}
                        size={props.inputSize}
                        autoComplete={props.autoComplete}
                        maxLength={props.maxLength}
                        inputRef={props.inputRef}
                    />
                    {this.getCounter(props.counter)}
                    <Tooltip
                        messages={errorMessages}
                        type={props.errorSeverity}
                        className={props.placement}
                        tooltipPlace={props.tooltipPlace}
                        rendered={props.error} />
                </div>
            </div>
        );
    }
}

SingleLineInputWithError.propTypes = {
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
    errorSeverity: PropTypes.oneOf(["error", "warning"]),
    counter: PropTypes.number,
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
    type: PropTypes.string,
    style: PropTypes.object,
    labelStyle: PropTypes.object,
    extraToolTips: PropTypes.node,
    autoComplete: PropTypes.oneOf(["off", "on"]),
    maxLength: PropTypes.number
};
SingleLineInputWithError.defaultProps = {
    error: false,
    enabled: true,
    className: "",
    placement: "inside",
    inputSize: "small",
    labelType: "block",
    errorMessage: ["This field has an error."],
    errorSeverity: "error",
    autoComplete: "on"
};

export default SingleLineInputWithError;