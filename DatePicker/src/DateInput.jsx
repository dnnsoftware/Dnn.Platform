import React, {Component, PropTypes} from "react";

const Periods = ["AM", "PM"];
const numberReg = new RegExp("^\\d+$");

export default class DateInput extends Component {
    constructor(props) {
        super(props);

        const month = this.addZero(this.props.date.getMonth() + 1);
        const day = this.addZero(this.props.date.getDate());
        const year = this.props.date.getFullYear();
        const hours = this.getHour(this.props.date.getHours());
        const minutes = this.addZero(this.props.date.getMinutes());
        const period = this.getPeriod(this.props.date.getHours());

        this.state = { month, day, year, hours, minutes, period };

    }

    componentWillReceiveProps(props) {
        const month = this.addZero(props.date.getMonth() + 1);
        const day = this.addZero(props.date.getDate());
        const year = props.date.getFullYear();
        const hours = this.getHour(props.date.getHours());
        const minutes = this.addZero(props.date.getMinutes());
        const period = this.getPeriod(props.date.getHours());

        this.state = { month, day, year, hours, minutes, period };
    }


    setDate() {
        let date = this.props.date;
        date.setMonth(this.state.month - 1);
        date.setDate(this.state.day);
        date.setFullYear(this.state.year);
        date.setHours(this.state.hours);
        date.setMinutes(this.state.minutes);
        this.props.onUpdateDate(date);
    }

    setMonth() {
        let {month} = this.state;
        if (month > 12) {
            month = 12;
        }
        if (month < 1) {
            month = 1;
        }
        month = this.addZero(month);
        this.setState({ month });
        let date = this.props.date;
        date.setMonth(month - 1);
        this.props.onUpdateDate(date);
    }

    setDay() {
        let {day} = this.state;
        if (day > 31) {
            day = 31;
        }
        if (day < 1) {
            day = 1;
        }
        day = this.addZero(day);
        this.setState({ day });
        let date = this.props.date;
        date.setDate(day);
        this.props.onUpdateDate(date);
    }

    setHours() {
        let {hours} = this.state;
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
        let date = this.props.date;
        date.setHours(hours);
        this.props.onUpdateDate(date);
    }

    setMinutes() {
        let {minutes} = this.state;
        if (minutes > 59) {
            minutes = 59;
        }
        if (minutes < 0) {
            minutes = 0;
        }
        minutes = this.addZero(minutes);
        this.setState({ minutes });
        let date = this.props.date;
        date.setMinutes(minutes);
        this.props.onUpdateDate(date);
    }

    setYear() {
        let {year} = this.state;
        year = this.formatYear(year);
        this.setState({ year });
        let date = this.props.date;
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
        if (hour > 11) {
            return "PM";
        }
        return "AM";
    }

    setPeriod(e) {
        const value = e.target.value;
        this.setState({ period: value });
        let date = this.props.date;
        let hours = date.getHours();
        if (value === "PM" && hours < 12) {
            hours = +hours + 12;
        }
        if (value === "AM" && hours >= 12) {
            hours -= 12;
        }
        date.setHours(hours);
        this.props.onUpdateDate(date);
    }

    render() {
        const options = Periods.map(p => <option key={p} value={p}>{p}</option>);
        return <div className="date-input">

            <input type="text" value={this.state.month} onBlur={this.setMonth.bind(this) } onChange={this.updateValue.bind(this, "month") } placeholder="MM"/>
            <span>/</span>
            <input type="text" value={this.state.day} onBlur={this.setDay.bind(this) } onChange={this.updateValue.bind(this, "day") } placeholder="DD"/>
            <span>/</span>
            <input type="text" className="year" value={this.state.year} onBlur={this.setYear.bind(this) } onChange={this.updateValue.bind(this, "year") } placeholder="YYYY"/>
            {this.props.hasTimePicker && <div>
                <span>&nbsp; </span>
                <input type="text" value={this.state.hours} onBlur={this.setHours.bind(this) } onChange={this.updateValue.bind(this, "hours") } placeholder="hh"/>
                <span>: </span>
                <input type="text" value={this.state.minutes} onBlur={this.setMinutes.bind(this) } onChange={this.updateValue.bind(this, "minutes") } placeholder="mm"/>
                <span>&nbsp; </span>
                <select value={this.state.period} onChange={this.setPeriod.bind(this) }>
                    {options}
                </select>
            </div>}
        </div >;
    }
}


DateInput.propTypes = {
    date: PropTypes.instanceOf(Date),
    onUpdateDate: PropTypes.func.isRequired,
    hasTimePicker: PropTypes.bool
};

