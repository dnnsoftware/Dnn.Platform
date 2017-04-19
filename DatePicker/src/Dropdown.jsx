import React, { Component, PropTypes } from "react";

const MAX_LABEL_LENGHT = 20;

class Dropdown extends Component {

    constructor() {
        super();
        this.state = {
            isTimeSelectorVisible: false,
            className: ""
        };
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
    }

    handleClick(e) {
        if (!this._isMounted) { return; }
        const node = this.refs.timePicker;
        if (node && node.contains(e.target)) {
            return;
        }
        this.hideTimeSelector();
    }

    onUpdate(value) {
        this.hideTimeSelector();
        setTimeout( ()=> {this.props.onUpdate(value);}, 200);
    }
    
    showTimeSelector() {
        this.setState({ isTimeSelectorVisible: true }, () => {
            setTimeout(() => {
                this.setState({ className: "show" });
            }, 0);
        });
    }

    hideTimeSelector() {
        this.setState({ className: "" }, () => {
            setTimeout(() => {
                this.setState({ isTimeSelectorVisible: false });
            }, 200);
        });
    }

    getAbbreviatedLabel(label){
        return (label && label.length > MAX_LABEL_LENGHT) ? label.substring(0, MAX_LABEL_LENGHT - 3) + "..." : label;
    }

    render() {
        const {options, value, className} = this.props;
        const layoutOptions = options.map((option) => {
            return <div className="time-option" key={option.value} onClick={this.onUpdate.bind(this, option.value) }>{option.label}</div>;
        });
        const currentOption = options.find(o => o.value === value);
        
        return (
            <div className={"dnn-time-picker " +  (className || "")} ref="timePicker">
                <div className="time-text" onClick={this.showTimeSelector.bind(this)}>
                    <span title={currentOption.label}>{this.getAbbreviatedLabel(currentOption.label)}</span>
                    {this.state.isTimeSelectorVisible && <div className={"time-selector " + this.state.className}>
                        <div className="time-options">
                            {layoutOptions}
                        </div>
                    </div>}
                </div>
            </div >
        );
    }
}

Dropdown.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    options: PropTypes.array.isRequired,
    value: PropTypes.string,
    className: PropTypes.string
};

export default Dropdown;