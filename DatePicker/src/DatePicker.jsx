import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import DayPicker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import moment from "moment";
import TimePicker from "./TimePicker";
import DateInput from "./DateInput";
import "./style.less";

const DefaultControllerClassName = "calendar-controller";

function Weekday({ weekday, className, localeUtils, locale }) {
    const weekdayName = localeUtils.formatWeekdayLong(weekday, locale);
    return (
        <div className={className} title={weekdayName}>
            {weekdayName.slice(0, 1) }
        </div>
    );
}

function hasClass(element, className) {
    return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
}

Weekday.propTypes = WeekdayPropTypes;

const clearButtonStyleVisible = {
    transition: "300ms",
    marginRight: 0,
    opacity: 1
};

const clearButtonStyleInvisible = {
    transition: "300ms",
    marginRight: 0,
    opacity: 0
};

export default class DatePicker extends Component {

    constructor(props) {
        super(props);
        let firstDate = typeof props.date === "string" ? new Date(props.date) : props.date;

        let secondDate = typeof props.secondDate === "string" ? new Date(props.secondDate) : props.secondDate;

        this.savedDate = {
            FirstDate: firstDate !== undefined ? firstDate : null,
            SecondDate: secondDate !== undefined ? secondDate : null
        };

        this.state = {
            isCalendarVisible: false,
            Date: {
                FirstDate: firstDate !== undefined ? firstDate : null,
                SecondDate: secondDate !== undefined ? secondDate : null
            }
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
        const isController = hasClass(e.target, DefaultControllerClassName) || this.props.controllerClassName && hasClass(e.target, this.props.controllerClassName);

        if (!this._isMounted) { return; }
        const node = ReactDOM.findDOMNode(this);
        if (node && node.contains(e.target)) {
            return;
        }
        if (isController) {
            return;
        }
        this.cancel();
    }

    resetHours(date) {
        let newDate = date;
        newDate.setHours(0);
        newDate.setMinutes(0);
        newDate.setSeconds(0);
        return newDate;
    }

    disableDates(FirstDate, SecondDate, day, maxDay, minDay) {
        let maxDate = maxDay;
        let minDate = minDay;
        if (!FirstDate && !SecondDate && !maxDate && !minDate) {
            return;
        }
        if (SecondDate && maxDate) {
            maxDate = SecondDate < maxDate ? SecondDate : maxDate;
        } else {
            maxDate = SecondDate || maxDate;
        }
        if (FirstDate && minDate) {
            minDate = FirstDate > minDate ? FirstDate : minDate;
        } else {
            minDate = FirstDate || minDate;
        }
        const thisDay = this.resetHours(day);
        if (minDate && maxDate) {
            return thisDay < minDate || thisDay > maxDate;
        }
        if (minDate) {
            return thisDay < minDate;
        }
        if (maxDate) {
            return thisDay > maxDate;
        }
    }

    firstDisableDates(day) {
        const maxDate = this.props.maxDate ? this.resetHours(new Date(this.props.maxDate)) : null;
        const minDate = this.props.minDate ? this.resetHours(new Date(this.props.minDate)) : null;
        const SecondDate = this.state.Date.SecondDate ? this.resetHours(new Date(this.state.Date.SecondDate)) : null;
        return this.disableDates(null, SecondDate, day, maxDate, minDate);
    }

    secondDisableDates(day) {
        const FirstDate = this.state.Date.FirstDate ? this.resetHours(new Date(this.state.Date.FirstDate)) : null;
        const minDate = this.props.minSecondDate ? this.resetHours(new Date(this.props.minSecondDate)) : null;
        const maxDate = this.props.maxSecondDate ? this.resetHours(new Date(this.props.maxSecondDate)) : null;
        return this.disableDates(FirstDate, null, day, maxDate, minDate);
    }

    updateTime(date, timeDate) {
        if (!date) {
            return null;
        }
        if (!timeDate) {
            return date;
        }

        let newDate = new Date(date);
        const TimeDate = new Date(timeDate);
        const hours = TimeDate.getHours();
        const minutes = TimeDate.getMinutes();
        newDate.setHours(hours);
        newDate.setMinutes(minutes);
        return newDate;
    }

    updateDate(firstDate, secondDate, options = {}) {
        if (options.disabled) {
            return;
        }
        let FirstDate = firstDate;
        let SecondDate = secondDate;
        let {Date} = this.state;

        if (!options.preventUpdateTime && this.props.hasTimePicker) {
            if (FirstDate && Date.FirstDate) {
                FirstDate = this.updateTime(FirstDate, Date.FirstDate);
            }
            if (SecondDate && Date.SecondDate) {
                SecondDate = this.updateTime(SecondDate, Date.SecondDate);
            }
        }

        if (typeof this.props.date === "string") {
            if (FirstDate) {
                FirstDate = this.formatDate(FirstDate, "L") + " " + this.formatDate(FirstDate, "LT");
            }
            if (SecondDate) {
                SecondDate = this.formatDate(SecondDate, "L") + " " + this.formatDate(SecondDate, "LT");
            }
        }

        Date.FirstDate = FirstDate !== undefined ? FirstDate : Date.FirstDate;
        Date.SecondDate = SecondDate !== undefined ? SecondDate : Date.SecondDate;

        this.setState({
            Date
        });
        if (options.callUpdateDate || !this.props.isDateRange && !this.props.hasTimePicker) {
            this.callUpdateDate();
            if (!options.preventHide) {
                this.hideCalendar();
            }
        }
    }

    callUpdateDate() {
        let {Date} = this.state;
        this.props.updateDate(Date.FirstDate, Date.SecondDate);
        this.cashPreviousDates();
    }

    cashPreviousDates() {
        const {Date} = this.state;
        const FirstDate = Date.FirstDate;
        const SecondDate = Date.SecondDate;
        this.savedDate = { FirstDate, SecondDate };
    }

    apply() {
        this.callUpdateDate();
        this.hideCalendar();
    }

    cancel() {
        this.hideCalendar();
        const FirstDate = this.savedDate.FirstDate;
        const SecondDate = this.savedDate.SecondDate;
        this.setState({ Date: { FirstDate, SecondDate } });
    }

    updateFirstTime(time) {
        let date = this.date || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(date, this.secondDate, { preventUpdateTime: true });
    }

    updateSecondTime(time) {
        let date = this.secondDate || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(this.date, date, { preventUpdateTime: true });
    }

    firstDateClick(e, day, { disabled }) {
        this.updateDate(day, undefined, { disabled });
    }

    secondDateClick(e, day, { disabled }) {
        this.updateDate(undefined, day, { disabled });
    }

    formatDate(date, format = "dddd, MMMM, Do, YYYY") {
        if (date) {
            return moment(date).format(format);
        }
        return false;
    }

    showCalendar() {       
        this.setState({ isCalendarVisible: true });
    }

    hideCalendar() {
        this.setState({ isCalendarVisible: false });
    }

    toggleCalendar() {
        const isCalendarVisible = !this.state.isCalendarVisible;
        this.setState({ isCalendarVisible });
        if (typeof this.props.onIconClick === "function") {
            this.props.onIconClick();
        }
    }

    getStyle() {
        let style = { width: 256 };
        if (this.props.isDateRange) {
            style.width = 512;
        }
        return style;
    }

    updateFirstDate(date, preventUpdateTime=false) {
        const firstDate = date ? date : this.state.Date.FirstDate;
        const secondDate = this.state.Date.SecondDate;
        this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
    }

    updateSecondDate(date, preventUpdateTime=false) {
        const secondDate = date ? date : this.state.Date.SecondDate;
        const firstDate = this.state.Date.FirstDate;
        this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
    }

    clearDates() {
        let Date = {};
        Date.FirstDate = null;
        Date.SecondDate = null;
        this.setState({Date}, this.apply.bind(this));
    }

    onClearDatesPressed() {
        const Date = {FirstDate: null, SecondDate: null };        
        this.setState({ Date }, () => this.callUpdateDate());
    }
   
    render() {
        this.date = this.state.Date.FirstDate;
        this.secondDate = this.state.Date.SecondDate;

        let firstDate = this.date;
        let secondDate = this.secondDate;

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
        const calendarClassName = "calendar-container" + (this.state.isCalendarVisible ? " visible" : " invisible");

        firstDate = firstDate ? new Date(firstDate) : null;
        secondDate = secondDate ? new Date(secondDate) : null;

        const showIcon = this.props.showIcon !== false;
        const showInput = this.props.showInput !== false;

        const mode = this.props.mode ? "_" + this.props.mode : "";
        let icon = require(`!raw!./img/calendar${mode}.svg`);
        if (this.props.icon) {
            icon = this.props.icon;
        }

        const style = this.props.isDateRange && this.props.hasTimePicker ? {width: 380} : {}; 
        const buttonStyle = this.props.isDateRange ? {} : {margin: "10px auto", float: "none"};
        const inputClassName = "calendar-text" + ( this.props.hasTimePicker ? " with-time-picker" : "");

        const showCheckBox = !!this.props.isDateRange && this.props.showCheckBoxClearDates;
        const clearButtonStyle = (this.state.Date.FirstDate || this.state.Date.SecondDate) ? clearButtonStyleVisible : clearButtonStyleInvisible;            

        /* eslint-disable react/no-danger */
        return <div className="dnn-day-picker">
            {showInput && <div className={inputClassName} style={style} onClick={this.showCalendar.bind(this) }>
                {this.props.prependWith && <span>{this.props.prependWith}</span>}
                {this.props.showClearDateButton && <div className="clear-button" onClick={this.clearDates.bind(this)}>×</div>}
                {this.props.isInputReadOnly && displayDate}
                {!this.props.isInputReadOnly && <div style={{ float: "right" }}>
                    <DateInput date={firstDate} onUpdateDate={this.updateFirstDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    {this.props.isDateRange && <div>
                        <span>&nbsp; -&nbsp; </span>
                        <DateInput date={secondDate} onUpdateDate={this.updateSecondDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    </div>}
                </div>}
            </div>}
            {showIcon && <div
                dangerouslySetInnerHTML={{ __html: icon }}
                className={"calendar-icon" + (this.state.isCalendarVisible ? " active" : "") }
                onClick={this.toggleCalendar.bind(this) }>
            </div>}
            <div className={calendarClassName} style={this.getStyle() }>
                <div>
                    <DayPicker
                        weekdayElement={ <Weekday/> }
                        initialMonth={firstDate || new Date() }
                        selectedDays={day => DateUtils.isSameDay(firstDate, day) }
                        onDayClick={this.firstDateClick.bind(this) }
                        disabledDays={ this.firstDisableDates.bind(this) }
                        />
                    {this.props.hasTimePicker && <TimePicker updateTime={this.updateFirstTime.bind(this) } time={this.formatDate(this.date, "LT") }/>}
                </div>

                {this.props.isDateRange && <div>
                    <DayPicker
                        weekdayElement={ <Weekday/> }
                        initialMonth={secondDate || new Date() }
                        selectedDays={day => DateUtils.isSameDay(secondDate, day) }
                        onDayClick={this.secondDateClick.bind(this) }
                        disabledDays={ this.secondDisableDates.bind(this) }
                        />
                    {this.props.hasTimePicker && <TimePicker updateTime={this.updateSecondTime.bind(this) } time={this.formatDate(this.secondDate, "LT") }/>}
                </div>}
                {showButton && <button style={buttonStyle} role="primary" onClick={this.apply.bind(this) }>{this.props.applyButtonText || "Apply"}</button>}
                {showCheckBox && <button role="primary" style={clearButtonStyle} onClick={this.onClearDatesPressed.bind(this)}>Clear</button> }                
            </div>
        </div>;
    }
}

DatePicker.propTypes = {
    // -----REQUIRED PROPS--------:
    date: PropTypes.instanceOf(Date),
    updateDate: PropTypes.func.isRequired,

    // -----OPTIONAL PROPS--------:

    // if set to true, it shows 2 calendars
    isDateRange: PropTypes.bool,

    //if isDateRange is true the secondDate is Required
    secondDate: PropTypes.instanceOf(Date),

    //min and max dates to reduce dates user can select. 
    minDate: PropTypes.instanceOf(Date),
    maxDate: PropTypes.instanceOf(Date),

    minSecondDate: PropTypes.instanceOf(Date),
    maxSecondDate: PropTypes.instanceOf(Date),

    // if set to true, it shows time picker 
    hasTimePicker: PropTypes.bool,


    //if set to true it shows static text insted of input fields
    isInputReadOnly: PropTypes.bool,

    //show/hide an icon
    showIcon: PropTypes.bool,

    //function that will be called when icon is clicked
    onIconClick: PropTypes.func,

    //icon mode: "start" or "end". Default icon shows up if mode or custom icon is not provided
    mode: PropTypes.string,

    //custom icon
    icon: PropTypes.string,

    //show/hide input
    showInput: PropTypes.bool,

    applyButtonText: PropTypes.string,

    //to be able to click on element without hiding the calendar it's needed to provide class name of a controller or give to controller a default className - "calendar-controller"
    controllerClassName: PropTypes.string,

    showClearDateButton:  PropTypes.bool,

    showCheckBoxClearDates: PropTypes.bool.isRequired,

    prependWith: PropTypes.string
};

DatePicker.defaultProps = {
    showCheckBoxClearDates: false
};