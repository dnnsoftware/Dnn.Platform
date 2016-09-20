import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Checkbox extends Component {

    componentWillMount() {
        const {props} = this;
        this.setState({
            checked: props.value
        });
        this.id = "checkbox-" + Math.random() + Date.now();
    }

    componentWillReceiveProps(props) {
        this.setState({
            checked: props.value
        });
    }

    onChange() {
        if (typeof this.props.onChange === "function") {
            this.props.onChange(!this.state.checked);
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
            <div className={"dnn-checkbox-container " + props.labelPlace}>
                <div className={className} style={Object.assign(checkBoxStyle, props.style) }>
                    <input
                        type="checkbox"
                        id={this.id}
                        checked={props.value}
                        onChange={this.onChange.bind(this) } />
                    <label htmlFor={this.id}></label>
                </div>
                {!!label && <label htmlFor={this.id}>{label}</label>}
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
    onChange: PropTypes.func
};

Checkbox.defaultProps = {
    size: 17,
    labelPlace: "right"
};