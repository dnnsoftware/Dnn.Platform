import React, {PropTypes, Component} from "react";
import "./style.less";

class RadioButtons extends Component {
    componentWillMount() {
        const {props} = this;
        this.setState({
            value: props.value
        });
    }
    componentWillReceiveProps(props) {
        this.setState({
            value: props.value
        });
    }
    onChange(event) {
        const {props} = this;
        const value = event.target.value;
        props.onChange(value);
    }
    render() {
        const {props, state} = this;
        const buttons = props.options.map((button) => {
            const uniqueKey = "radio-button-" + button.label + "-" + button.value;
            const checked = (button.value && button.value.toString()) === (state.value && state.value.toString());
            const radioButtonClass = (props.disabled ? "disabled" : "") + (checked ? " checked" : "");
            return (
                <li key={uniqueKey} className={radioButtonClass} style={{ width: props.buttonWidth }}>
                    <input type="radio" id={uniqueKey} onChange={this.onChange.bind(this) } value={button.value} name={props.buttonGroup} checked={checked} disabled={props.disabled}/>
                    <div className="check"><div className="inside"></div></div>
                    <label htmlFor={uniqueKey} disabled={props.disabled}>{button.label}</label>
                </li>);
        });
        return (
            <div className="dnn-radio-buttons">
                <label>{props.label}</label>
                <ul>
                    {buttons}
                </ul>
            </div>
        );
    }
}

RadioButtons.PropTypes = {
    onChange: PropTypes.func,
    label: PropTypes.string,
    options: PropTypes.array.isRequired,
    buttonGroup: PropTypes.string,
    buttonWidth: PropTypes.number,
    disabled: PropTypes.bool,
    value: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
};

export default RadioButtons;