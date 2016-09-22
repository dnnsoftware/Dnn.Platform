import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import DayPicker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import moment from "moment";
import TimePicker from "./TimePicker";
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
        const node = ReactDOM.findDOMNode(this);
        if (node && node.contains(e.target)) {
            return;
        }
        if (this.props.showInput === false) {
            return;
        }
        this.hideCalendar();
    }


    firstDisableDates(day) {
        if (!this.secondDate) {
            return false;
        }
        return day > this.secondDate;
    }

    secondDisableDates(day) {
        if (!this.date) {
            return false;
        }
        return day < this.date;
    }

    updateDate(firstDate, secondDate, disabled) {
        if (disabled) {
            return;
        }
        let FirstDate = firstDate;
        let SecondDate = secondDate;
        if (typeof this.props.date === "string") {
            if (FirstDate) {
                FirstDate = this.formatDate(FirstDate, "L") + " " + this.formatDate(FirstDate, "LT");
            }
            if (SecondDate) {
                SecondDate = this.formatDate(SecondDate, "L") + " " + this.formatDate(SecondDate, "LT");
            }
        }
        this.props.updateDate(FirstDate, SecondDate);
        if (!this.props.isDateRange && !this.props.hasTimePicker) {
            this.hideCalendar();
        }
    }

    updateFirstTime(time) {
        const date = new Date(this.formatDate(this.date, "L") + " " + time);
        this.updateDate(date, this.secondDate);
    }

    updateSecondTime(time) {
        const secondDate = new Date(this.formatDate(this.secondDate, "L") + " " + time);
        this.updateDate(this.date, secondDate);
    }

    firstDateClick(e, day, { disabled }) {
        this.updateDate(day, undefined, disabled);
    }
    
    secondDateClick(e, day, { disabled }) {
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
        if (this.props.hideCalendar) {
            return this.props.hideCalendar();
        }
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
        this.date = typeof this.props.date === "string" ? new Date(this.props.date) : this.props.date;
        this.secondDate = typeof this.props.secondDate === "string" ? new Date(this.props.secondDate) : this.props.secondDate;
        
        const firstDate = this.date;
        const secondDate = this.secondDate;
        let displayFirstDate = firstDate ? this.formatDate(firstDate, "L") : "";
        let displaySecondDate = secondDate ? this.formatDate(secondDate, "L") : "";

        if (this.props.hasTimePicker) {
            displayFirstDate += (displayFirstDate ? " " + this.formatDate(firstDate, "LT") : "");
            displaySecondDate += (displaySecondDate ? " " + this.formatDate(secondDate, "LT") : "");
        }
        let displayDate = displayFirstDate;
        if (this.props.isDateRange && secondDate) {
            displayDate += " - " + displaySecondDate;
        }
        const showButton = !!this.props.isDateRange || !!this.props.hasTimePicker;
        const showCalendar = this.state.isCalendarVisible || this.props.isCalendarVisible;

        const showInput = this.props.showInput !== false;
        const calendarClassName = "calendar-container " + this.state.className + (this.props.isCalendarVisible ? " show" : "");
        return <div className="dnn-day-picker">
            {showInput && <div className={"calendar-icon" + (this.state.className ? " active" : "") } onClick={this.toggleCalendar.bind(this) }></div>}
            {showInput && <div className="calendar-text" onClick={this.showCalendar.bind(this) }>
                {displayDate}
            </div>}
            {showCalendar &&
                <div className={calendarClassName} style={this.getStyle() }>
                    <div>
                        <DayPicker
                            weekdayElement={ <Weekday/> }
                            initialMonth={firstDate || new Date() }
                            selectedDays={day => DateUtils.isSameDay(firstDate, day) }
                            onDayClick={this.firstDateClick.bind(this) }
                            disabledDays={ this.firstDisableDates.bind(this) }
                            />
                        {this.props.hasTimePicker && <TimePicker updateTime={this.updateFirstTime.bind(this) } time={this.formatDate(this.date, "LT")}/>}
                    </div>

                    {this.props.isDateRange && <div>
                        <DayPicker
                            weekdayElement={ <Weekday/> }
                            initialMonth={secondDate || new Date() }
                            selectedDays={day => DateUtils.isSameDay(secondDate, day) }
                            onDayClick={this.secondDateClick.bind(this) }
                            disabledDays={ this.secondDisableDates.bind(this) }
                            />
                        {this.props.hasTimePicker && <TimePicker updateTime={this.updateSecondTime.bind(this) } time={this.formatDate(this.secondDate, "LT")}/>}
                    </div>}
                    {showButton && <button role="primary" onClick={this.hideCalendar.bind(this) }>Apply</button>}
                </div>}
        </div >;
    }
}

DatePicker.propTypes = {
    // Required Props
    date: PropTypes.instanceOf(Date),
    updateDate: PropTypes.func.isRequired,
    
    // Optional Props
    secondDate: PropTypes.instanceOf(Date),
    isDateRange: PropTypes.bool,
    hasTimePicker: PropTypes.bool,

    //if showInput is false the controll of showing/hiding the calendar 
    // should be performed outside of the component. 
    //In this case isCalendarVisible.bool and hideCalendar.func are Required 
    showInput: PropTypes.bool,
    isCalendarVisible: PropTypes.bool,
    hideCalendar: PropTypes.func
};