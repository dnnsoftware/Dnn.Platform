import React, { Component } from "react";
import PropTypes from "prop-types";
import Tooltip from "../Tooltip";
import "./style.less";

class RadioButtons extends Component {
    
    constructor(props) {
        super(props);
        this.state = {
            value: props.value
        };
    }    

    componentDidUpdate(prevProp) {
        if (this.props.value !== prevProp.value) {
            this.setState({
                value: this.props.value
            });
        }
    }

    onChange(event) {
        const {props} = this;
        const value = event.target.value;
        props.onChange(value);
    }

    render() {
        const {props, state} = this;
        const tooltipMessages = props.tooltipMessage instanceof Array ? props.tooltipMessage : [props.tooltipMessage];
        const buttons = props.options.map((button) => {
            const uniqueKey = "radio-button-" + (props.id ? props.id + "-" : "") + button.label + "-" + button.value;
            const checked = (button.value && button.value.toString()) === (state.value && state.value.toString());
            const radioButtonClass = (props.disabled ? "disabled" : "") + (checked ? " checked" : "");
            return (
                <li key={uniqueKey} className={radioButtonClass} style={{ width: props.buttonWidth }}>
                    <input type="radio" id={uniqueKey} onChange={this.onChange.bind(this)} value={button.value} name={props.buttonGroup} checked={checked} disabled={props.disabled} />
                    <div className="check"></div>
                    <label htmlFor={uniqueKey} disabled={props.disabled}>{button.label}</label>
                </li>);
        });
        return (
            <div className={"dnn-radio-buttons " + props.float}>
                {!!props.label && <label>{props.label}</label>}
                <Tooltip
                    messages={tooltipMessages}
                    type="info"
                    className={props.placement}
                    tooltipPlace={props.tooltipPlace}
                    rendered={props.tooltipMessage} />
                <ul>
                    {buttons}
                </ul>
            </div>
        );
    }
}

RadioButtons.propTypes = {
    onChange: PropTypes.func,
    label: PropTypes.string,
    options: PropTypes.array.isRequired,
    buttonGroup: PropTypes.string,
    buttonWidth: PropTypes.number,
    disabled: PropTypes.bool,
    value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    placement: PropTypes.string,
    tooltipPlace: PropTypes.string,
    float: PropTypes.string,
    id: PropTypes.string
};

RadioButtons.defaultProps = {
    float: "left",
    value: ""
};

export default RadioButtons;