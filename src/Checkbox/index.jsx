import React, { Component } from "react";
import PropTypes from "prop-types";
import Label from "../Label";
import "./style.less";

export default class Checkbox extends Component {

    constructor(props) {
        super(props);
        this.state = { checked: props.value ? props.value : false };
        this.id = "checkbox-" + Math.random() + Date.now();
    }

    componentDidUpdate(prevProps) {
        if (this.props.value !== prevProps.value) {
            this.setState({
                checked: this.props.value
            });
        }
    }

    onClick() {
        if (typeof this.props.onChange === "function" && this.props.enabled) {
            this.setState({
                checked: !this.state.checked
            }, () => {
                this.props.onChange(this.state.checked);
            });
        }
    }

    render() {
        const className = "checkbox";
        let label = this.props.label || "";
        const {props} = this;
        const checkBoxStyle = {
            width: props.size,
            height: props.size
        };
        return (
            <div className={"dnn-checkbox-container " + (props.labelPlace ? "align-" + props.labelPlace : "") + (!this.props.enabled ? " disabled" : "")}>
                {(!!label && props.labelPlace === "left") &&
                    <Label
                        labelFor={this.id}
                        onClick={this.onClick.bind(this)}
                        label={label}
                        tooltipMessage={props.tooltipMessage}
                        tooltipPlace={props.tooltipPlace}
                        tooltipStyle={{float: "none", display: "inline-block"}}
                        style={{float: "none", display: "inline-block", width: "auto"}}
                    />
                }
                <div className={className} style={Object.assign(checkBoxStyle, props.style)}>
                    <input
                        type="checkbox"
                        id={this.id}
                        checked={this.state.checked}
                        onChange={() => {}}
                        aria-label={label ? "" : "Select"}
                    />
                    <label htmlFor={this.id} onClick={this.onClick.bind(this)}>Check</label>
                </div>
                {(!!label && props.labelPlace === "right") &&
                    <Label
                        labelFor={this.id}
                        onClick={this.onClick.bind(this)}
                        label={label}
                        tooltipMessage={props.tooltipMessage}
                        tooltipPlace={props.tooltipPlace}
                        tooltipStyle={{float: "none", display: "inline-block"}}
                        style={{float: "none", display: "inline-block", width: "auto"}}
                    />
                }
            </div>
        );
    }
}

Checkbox.propTypes = {
    value: PropTypes.bool.isRequired,
    labelPlace: PropTypes.oneOf(["left", "right"]),
    size: PropTypes.number,
    checkBoxStyle: PropTypes.object,
    label: PropTypes.string,
    onChange: PropTypes.func,
    enabled: PropTypes.bool,
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    tooltipPlace: PropTypes.string
};

Checkbox.defaultProps = {
    enabled: true,
    size: 13,
    labelPlace: "right"
};