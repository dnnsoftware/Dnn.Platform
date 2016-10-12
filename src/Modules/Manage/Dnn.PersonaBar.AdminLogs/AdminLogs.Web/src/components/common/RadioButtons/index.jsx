import React, {PropTypes, Component} from "react";
import "./style.less";

class RadioButtons extends Component {
    render() {
        const {props} = this;
        const buttons = props.options.map((button) => {
            const uniqueKey=  "radio-button-" + button.label + "-" + button.value;
            const checked = button.value.toString() === props.value.toString();
            return (
                <li key={uniqueKey}>
                    <input type="radio" id={uniqueKey} onChange={props.onChange.bind(this) } value={button.value} name={props.buttonGroup} defaultChecked={checked}/>
                    <label htmlFor={uniqueKey}>{button.label}</label>

                    <div className="check"><div className="inside"></div></div>
                </li>);
        });
        return (
            <div className="radio-buttons">
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
    value: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
};

export default RadioButtons;