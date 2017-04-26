import React, { Component, PropTypes } from "react";

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

    render() {
        const {options, label, className} = this.props;
        const layoutOptions = options.map((option) => {
            return <div className="time-option" key={option.value} onClick={this.onUpdate.bind(this, option.value) }>{option.label}</div>;
        });
        
        return (
            <div className={"dnn-time-picker " +  (className || "")} ref="timePicker">
                <div className="time-text" onClick={this.showTimeSelector.bind(this)}>
                    <span className="time-text-label" title={label}>{label}</span>
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
    label: PropTypes.string,
    className: PropTypes.string
};

export default Dropdown;