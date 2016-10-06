import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Checkbox extends Component {

    constructor() {
        super();

        this.id = "checkbox-" + Math.random() + Date.now();
    }

    componentWillMount() {
        const {props} = this;
        this.setState({
            checked: props.value
        });
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
            <div className={"dnn-checkbox-container " + props.labelPlace + (!this.props.enabled ? " disabled" : "") }>
                <div className={className} style={Object.assign(checkBoxStyle, props.style) }>
                    <input
                        type="checkbox"
                        id={this.id}
                        checked={this.state.checked}
                        />
                    <label htmlFor={this.id} onClick={this.onClick.bind(this) }></label>
                </div>
                {!!label && <label htmlFor={this.id} onClick={this.onClick.bind(this) }>{label}</label>}
            </div>
        );
    }
}

Checkbox.propTypes = {
    value: PropTypes.object.isRequired,
    labelPlace: PropTypes.oneOf(["left", "right"]),
    size: PropTypes.number,
    checkBoxStyle: PropTypes.object,
    label: PropTypes.string,
    onChange: PropTypes.func,
    enabled: PropTypes.bool
};

Checkbox.defaultProps = {
    enabled: true,
    size: 17,
    labelPlace: "right"
};