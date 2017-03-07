import React, {Component, PropTypes} from "react";

const Periods = ["AM", "PM"];
const numberReg = new RegExp("^\\d+$");

export default class DateInput extends Component {
    constructor(props) {
        super(props);

        const month = this.props.date ? this.addZero(this.props.date.getMonth() + 1) : "";
        const day = this.props.date ? this.addZero(this.props.date.getDate()) : "";
        const year = this.props.date ? this.props.date.getFullYear() : "";
        const hours = this.props.date ? this.getHour(this.props.date.getHours()) : "";
        const minutes = this.props.date ? this.addZero(this.props.date.getMinutes()) : "";
        const period = this.props.date ? this.getPeriod(this.props.date.getHours()) : this.getPeriod(false);

        this.state = { month, day, year, hours, minutes, period };

    }

    componentWillReceiveProps(props) {
        const month = props.date ? this.addZero(props.date.getMonth() + 1) : "";
        const day = props.date ? this.addZero(props.date.getDate()) : "";
        const year = props.date ? props.date.getFullYear() : "";
        const hours = props.date ? this.getHour(props.date.getHours()) : "";
        const minutes = props.date ? this.addZero(props.date.getMinutes()) : "";
        const period = props.date ? this.getPeriod(props.date.getHours()) : this.getPeriod(false);

        this.state = { month, day, year, hours, minutes, period };
    }


    setMonth() {
        let {month} = this.state;
        if (!month) {
            return;
        }
        if (month > 12) {
            month = 12;
        }
        if (month < 1) {
            month = 1;
        }
        month = this.addZero(month);
        this.setState({ month });
        let date = this.props.date || new Date();
        date.setMonth(month - 1);
        this.props.onUpdateDate(date);
    }

    setDay() {

        let {day} = this.state;
        if (!day) {
            return;
        }
        if (day > 31) {
            day = 31;
        }
        if (day < 1) {
            day = 1;
        }
        day = this.addZero(day);
        this.setState({ day });
        let date = this.props.date || new Date();
        date.setDate(day);
        this.props.onUpdateDate(date);
    }

    setHours() {
        let {hours} = this.state;
        if (!hours) {
            return;
        }
        if (hours > 12) {
            hours = 12;
        }
        if (hours < 0) {
            hours = 0;
        }
        hours = this.addZero(hours);
        this.setState({ hours });
        if (this.state.period === "PM" && hours < 12) {
            hours = +hours + 12;
        }
        let date = this.props.date || new Date();
        date.setHours(hours);
        this.props.onUpdateDate(date, true);
    }

    setMinutes() {
        let {minutes} = this.state;
        if (!minutes) {
            return;
        }
        if (minutes > 59) {
            minutes = 59;
        }
        if (minutes < 0) {
            minutes = 0;
        }
        minutes = this.addZero(minutes);
        this.setState({ minutes });
        let date = this.props.date || new Date();
        date.setMinutes(minutes);
        this.props.onUpdateDate(date, true);
    }

    setYear() {
        let {year} = this.state;
        if (!year) {
            return;
        }
        year = this.formatYear(year);
        this.setState({ year });
        let date = this.props.date || new Date();
        date.setFullYear(year);
        this.props.onUpdateDate(date);
    }

    updateValue(key, e) {
        let value = e.target.value;
        if (value !== "" && !numberReg.test(value)) {
            return;
        }
        if (e.target.value.length > 4 || key !== "year" && e.target.value.length > 2) {
            return;
        }
        this.setState({ [key]: value });
    }


    getHour(number) {
        let hour = number;
        if (number > 12) {
            hour = number - 12;
        }
        return this.addZero(hour);

    }

    addZero(number) {
        const string = number + "";
        if (string.length < 2) {
            return "0" + number;
        }
        return number;
    }

    formatYear(number) {
        const string = number + "";
        if (string.length < 2) {
            return "000" + number;
        }
        if (string.length < 3) {
            return "00" + number;
        }
        if (string.length < 4) {
            return "0" + number;
        }
        return number;
    }

    getPeriod(hour) {
        if (hour === false && this.state && this.state.period) {
            return this.state.period;
        }
        
        if (hour > 11) {
            return "PM";
        }
        return "AM";
    }

    setPeriod(e) {
        e.preventDefault();
        const period = Periods.find(p => p !== this.state.period);
        this.setState({ period });
        let date = this.props.date;
        if (!date) {
            return;
        }
        let hours = date.getHours();
        if (period === "PM" && hours < 12) {
            hours = +hours + 12;
        }
        if (period === "AM" && hours >= 12) {
            hours -= 12;
        }
        date.setHours(hours);
        this.props.onUpdateDate(date, true);
    }

    render() {
        const className = "date-input" + (this.props.hasTimePicker ? " with-time-picker" : "" );
        return <div className={className}>
            <input type="text" value={this.state.month} onBlur={this.setMonth.bind(this) } onChange={this.updateValue.bind(this, "month") } placeholder="MM" aria-label="Month"/>
            <span>/</span>
            <input type="text" value={this.state.day} onBlur={this.setDay.bind(this) } onChange={this.updateValue.bind(this, "day") } placeholder="DD" aria-label="Day"/>
            <span>/</span>
            <input type="text" className="year" value={this.state.year} onBlur={this.setYear.bind(this) } onChange={this.updateValue.bind(this, "year") } placeholder="YYYY" aria-label="Year"/>
            {this.props.hasTimePicker && <div>
                <span>&nbsp; </span>
                <input type="text" value={this.state.hours} onBlur={this.setHours.bind(this) } onChange={this.updateValue.bind(this, "hours") } placeholder="hh" aria-label="Hour"/>
                <span>: </span>
                <input type="text" value={this.state.minutes} onBlur={this.setMinutes.bind(this) } onChange={this.updateValue.bind(this, "minutes") } placeholder="mm" aria-label="Minute"/>
                <span>&nbsp; </span>
                <div className="select-container" onClick={this.setPeriod.bind(this)}>
                    <span>{this.state.period}</span>
                </div>
            </div>}
        </div >;
    }
}


DateInput.propTypes = {
    date: PropTypes.instanceOf(Date),
    onUpdateDate: PropTypes.func.isRequired,
    hasTimePicker: PropTypes.bool
};