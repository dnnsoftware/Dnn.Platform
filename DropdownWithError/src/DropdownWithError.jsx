import React, {Component, PropTypes } from "react";
import Tooltip from "dnn-tooltip";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import "./style.less";

class DropdownWithError extends Component {
    constructor() {
        super();
    }

    render() {
        const {props} = this;
        const className = "dnn-dropdown-with-error" + (props.error ? " error" : "") + (" " + props.className) + (props.enabled ? "" : " disabled");
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
                <div className={"dropdown-tooltip-container " + props.labelType}>
                    <Dropdown
                        label={props.defaultDropdownValue}
                        fixedHeight={props.fixedHeight}
                        collapsibleWidth={props.collapsibleWidth}
                        collapsibleHeight={props.collapsibleHeight}
                        keepCollapsedContent={props.keepCollapsedContent}
                        scrollAreaStyle={props.scrollAreaStyle}
                        options={props.options}
                        onSelect={props.onSelect}
                        size={props.dropdownSize}
                        withBorder={props.withBorder}
                        withIcon={props.withIcon}
                        enabled={props.enabled}
                        value={props.value}
                        closeOnClick={props.closeOnClick}
                        labelIsMultiLine={props.labelIsMultiLine}
                        prependWith={props.prependWith}
                        title={props.title}
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

DropdownWithError.propTypes = {
    inputId: PropTypes.string,
    label: PropTypes.string,
    infoTooltipClassName: PropTypes.string,
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    infoTooltipPlace: PropTypes.string,
    labelType: PropTypes.string,
    className: PropTypes.string,
    dropdownSize: PropTypes.oneOf(["large", "small"]),
    error: PropTypes.bool,
    errorMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    tooltipPlace: PropTypes.string,
    placement: PropTypes.oneOf(["outside", "inside"]),
    defaultDropdownValue: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    options: PropTypes.array,
    onSelect: PropTypes.func,
    size: PropTypes.string,
    withBorder: PropTypes.bool,
    withIcon: PropTypes.bool,
    enabled: PropTypes.bool,
    value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
    closeOnClick: PropTypes.bool,
    style: PropTypes.object,
    labelStyle: PropTypes.object,
    extraToolTips: PropTypes.node,
    prependWith: PropTypes.string,
    labelIsMultiLine: PropTypes.bool,
    title: PropTypes.string
};
DropdownWithError.defaultProps = {
    error: false,
    enabled: true,
    className: "",
    placement: "outside",
    inputSize: "small",
    labelType: "block",
    errorMessage: ["This field has an error."],
    prependWith: ""
};
export default DropdownWithError;