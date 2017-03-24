import React, {Component, PropTypes} from "react";

let timeArray = [];
for (let i = 0; i < 24; i++) {
    const hour = getHour(i);
    for (let j = 0; j < 2; j++) {
        timeArray.push(hour + ":" + (j ? "30" : "00") + " " + (i > 11 ? "PM" : "AM"));
    }
}

function getHour(number) {
    let hour = number;
    if (number > 12) {
        hour = number - 12;
    }
    if (hour < 10) {
        hour = "0" + hour;
    }
    return hour;
}

export default class TimePicker extends Component {
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

    updateTime(time) {
        this.hideTimeSelector();
        setTimeout( ()=> {this.props.updateTime(time);}, 200);
    }

    toggleTimeSelector() {
        if (this.state.isTimeSelectorVisible) {
            return this.hideTimeSelector();
        }
        this.showTimeSelector();
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
        const TimeOptions = timeArray.map((time) => {
            return <div className="time-option" key={time} onClick={this.updateTime.bind(this, time) }>{time}</div>;
        });
        return <div className="dnn-time-picker" ref="timePicker">
            <div className="time-text" onClick={this.showTimeSelector.bind(this)}>
                {this.props.time}
                {this.state.isTimeSelectorVisible && <div className={"time-selector " + this.state.className}>
                    <div className="time-options">
                        {TimeOptions}
                    </div>
                </div>}
            </div>
        </div >;
    }
}

TimePicker.propTypes = {
    updateTime: PropTypes.func.isRequired,
    time: PropTypes.string
};