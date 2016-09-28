import React, { Component, PropTypes } from "react";
import "./style.less";

class Switch extends Component {

    constructor(props) {
        super(props);

        this.state = {
            switchActive: props.value,
            innerStateSet: false
        };
    }

    componentWillReceiveProps(props) {
        this.setState({
            switchActive: props.value
        });
    }

    toggleStatus() {
        const {props, state} = this;

        if (props.readOnly) {
            return;
        }

        if (typeof this.props.onChange === "function") {
            props.onChange(!state.switchActive);
        }
    }

    getClassName() {
        const {props} = this;

        let className = "dnn-switch";
        if (props.value) {
            className += " dnn-switch-active";
        }

        if (props.readOnly) {
            className += " dnn-switch-readonly";
        }        
        if (props.labelPlacement) {
            className += (" place-" + props.labelPlacement);
        }

        return className;
    }

    render() {
        const {props, state} = this;
        return (
            <div className="dnn-switch-container">
                {props.label && <span className="switch-label">{props.label}</span>}
                <span className={this.getClassName() } onClick={this.toggleStatus.bind(this) }>
                    <span className="mark" />
                    {!props.labelHidden && <label>{(state.switchActive ? props.onText : props.offText) }</label>}
                </span>
            </div>
        );
    }
}

Switch.propTypes = {
    value: PropTypes.bool,
    labelHidden: PropTypes.bool,
    onText: PropTypes.string,
    offText: PropTypes.string,
    label: PropTypes.string,
    onChange: PropTypes.func,
    readOnly: PropTypes.bool,
    labelPlacement: PropTypes.oneOf(["left", "right"])
};

Switch.defaultProps = {
    onText: "On",
    offText: "Off",
    labelPlacement: "left"
};

export default Switch;