import React, {PropTypes, Component} from "react";
import "./style.less";

class RadioButtons extends Component {
    componentWillMount() {
        const {props} = this;
        this.setState({
            value: props.defaultValue
        });
    }
    onChange(event) {
        const {props} = this;
        const value = event.target.value;
        this.setState({
            value
        });
        props.onChange(value);
    }
    render() {
        const {props, state} = this;
        const buttons = props.options.map((button) => {
            const uniqueKey = "radio-button-" + button.label + "-" + button.value;
            const checked = button.value.toString() === state.value.toString();
            const radioButtonClass = (props.disabled ? "disabled" : "") + (checked ? " checked" : "");
            return (
                <li key={uniqueKey} className={radioButtonClass} style={{ width: props.buttonWidth }}>
                    <input type="radio" id={uniqueKey} onChange={this.onChange.bind(this) } value={button.value} name={props.buttonGroup} defaultChecked={checked} disabled={props.disabled}/>
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
    defaultValue: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
};

export default RadioButtons;