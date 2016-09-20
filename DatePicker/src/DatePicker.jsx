import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import DayPicker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import moment from "moment";
import TimePicker from "./timePicker";
import "./style.less";

function Weekday({ weekday, className, localeUtils, locale }) {
    const weekdayName = localeUtils.formatWeekdayLong(weekday, locale);
    return (
        <div className={className} title={weekdayName}>
            {weekdayName.slice(0, 1) }
        </div>
    );
}

Weekday.propTypes = WeekdayPropTypes;

export default class DatePicker extends Component {

    constructor() {
        super();
        this.state = {
            isCalendarVisible: false,
            className: "",
            minTime: "00:00 AM",
            maxTime: "00:00 AM"
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
        const node = ReactDOM.findDOMNode(this);
        if (node && node.contains(e.target)) {
            return;
        }
        this.hideCalendar();
    }


    minDisableDates(day) {
        if (!this.props.maxDate) {
            return false;
        }
        return day > this.props.maxDate;
    }

    maxDisableDates(day) {
        if (!this.props.date) {
            return false;
        }
        return day < this.props.date;
    }

    updateDate(minDate, maxDate, disabled) {
        if (disabled) {
            return;
        }
        let MinDate = minDate;
        let MaxDate = maxDate;
        if (this.props.hasTimePicker) {
            if (minDate && this.state.minTime) {
                MinDate = new Date(this.formatDate(minDate, "L") + " " + this.state.minTime);
            }
            if (maxDate && this.state.maxTime) {
                MaxDate = new Date(this.formatDate(maxDate, "L") + " " + this.state.maxTime);
            }
        }
        this.props.updateDate(MinDate, MaxDate);
        if (!this.props.isDateRange && !this.props.hasTimePicker) {
            this.hideCalendar();
        }
    }

    updateTime(time) {
        this.setState({ minTime: time },
            this.updateDate.bind(this, this.props.date, this.props.maxDate)
        );
    }


    updateMaxTime(time) {
        this.setState({ maxTime: time },
            this.updateDate.bind(this, this.props.date, this.props.maxDate)
        );
    }

    minDateClick(e, day, { disabled }) {
        this.updateDate(day, undefined, disabled);
    }
    maxDateClick(e, day, { disabled }) {
        this.updateDate(undefined, day, disabled);
    }

    formatDate(date, format = "dddd, MMMM, Do, YYYY") {
        if (date) {
            return moment(date).format(format);
        }
        return false;
    }

    toggleCalendar() {
        if (this.state.isCalendarVisible) {
            return this.hideCalendar();
        }
        this.showCalendar();
    }

    showCalendar() {
        this.setState({ isCalendarVisible: true }, () => {
            setTimeout(() => {
                this.setState({ className: "show" });
            }, 0);
        });
    }

    hideCalendar() {
        this.setState({ className: "" }, () => {
            setTimeout(() => {
                this.setState({ isCalendarVisible: false });
            }, 200);
        });
    }

    getStyle() {
        let style = { width: 256 };
        if (this.props.isDateRange) {
            style.width = 512;
        }
        return style;
    }

    render() {
        const minDate = this.props.date;
        const maxDate = this.props.maxDate;
        let displayMinDate = minDate ? this.formatDate(minDate, "L") : "";
        let displayMaxDate = maxDate ? this.formatDate(maxDate, "L") : "";
        
        if (this.props.hasTimePicker) {
            displayMinDate += (displayMinDate ? " " + this.state.minTime : "");
            displayMaxDate += (displayMaxDate ? " " + this.state.maxTime: "");
        }
        let displayDate = displayMinDate;
        if (this.props.isDateRange && maxDate) {
            displayDate += " - " + displayMaxDate;
        }
        const showButton = !!this.props.isDateRange || !!this.props.hasTimePicker;
        return <div className="dnn-day-picker">
            <div className={"calendar-icon" + (this.state.className ? " active" : "") } onClick={this.toggleCalendar.bind(this) }></div>
            <div className="calendar-text" onClick={this.showCalendar.bind(this) }>
                {displayDate}
            </div>
            {this.state.isCalendarVisible &&
                <div className={"calendar-container " + this.state.className} style={this.getStyle() }>
                    <div>
                        <DayPicker
                            weekdayElement={ <Weekday/> }
                            initialMonth={minDate || new Date() }
                            selectedDays={day => DateUtils.isSameDay(minDate, day) }
                            onDayClick={this.minDateClick.bind(this) }
                            disabledDays={ this.minDisableDates.bind(this) }
                            />
                        {this.props.hasTimePicker && <TimePicker updateTime={this.updateTime.bind(this) } time={this.state.minTime}/>}
                    </div>

                    {this.props.isDateRange && <div>
                        <DayPicker
                            weekdayElement={ <Weekday/> }
                            initialMonth={maxDate || new Date() }
                            selectedDays={day => DateUtils.isSameDay(maxDate, day) }
                            onDayClick={this.maxDateClick.bind(this) }
                            disabledDays={ this.maxDisableDates.bind(this) }
                            />
                        {this.props.hasTimePicker && <TimePicker updateTime={this.updateMaxTime.bind(this) } time={this.state.maxTime}/>}
                    </div>}
                    {showButton && <button role="primary" onClick={this.hideCalendar.bind(this) }>Apply</button>}
                </div>}
        </div >;
    }
}


DatePicker.propTypes = {
    date: PropTypes.instanceOf(Date),
    maxDate: PropTypes.instanceOf(Date),
    isDateRange: PropTypes.bool,
    updateDate: PropTypes.func.isRequired,
    hasTimePicker: PropTypes.bool
};

