import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Checkbox extends Component {

    componentWillMount() {
        this.id = "checkbox-" + Math.random() + Date.now();
    }

    onChange(event) {
        if (typeof this.props.onChange === "function") {
            this.props.onChange(event.target.checked);
        }
    }

    render() {
        const className = "checkbox";
        let label = this.props.label || "";
        return (
            <div className="dnn-checkbox-container">
                <div className={className}>
                    <input
                        type="checkbox"
                        id={this.id}
                        checked={this.props.checked}
                        onChange={this.onChange.bind(this) } />
                    <label htmlFor={this.id}></label>
                </div>
                {!!label && <label htmlFor={this.id}>{label}</label>}
            </div>
        );
    }
}

Checkbox.propTypes = {
    checked: PropTypes.object.isRequired,
    label: PropTypes.string,
    onChange: PropTypes.func
};