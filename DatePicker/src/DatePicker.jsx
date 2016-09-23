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
            isCalendarVisible: props.isCalendarVisible,
            Date: {
                FirstDate: firstDate !== undefined ? firstDate : null,
                SecondDate: secondDate !== undefined ? secondDate : null
            }

        };
        this.handleClick = this.handleClick.bind(this);
    }

    componentWillReceiveProps(newProps) {
        if (newProps.isCalendarVisible === undefined) {
            return;
        }
        this.setState({ isCalendarVisible: newProps.isCalendarVisible });
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
        e.preventDefault();
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

    firstDisableDates(day) {
        if (!this.state.Date.SecondDate) {
            return false;
        }
        const SecondDate = new Date(this.state.Date.SecondDate);
        return day > SecondDate;
    }

    secondDisableDates(day) {
        if (!this.state.Date.FirstDate) {
            return false;
        }
        const FirstDate = new Date(this.state.Date.FirstDate);
        return day < FirstDate;
    }

    updateDate(firstDate, secondDate, disabled, options = {}) {
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
        let {Date} = this.state;
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
        this.savedDate = {FirstDate, SecondDate};
    }

    apply() {
        this.callUpdateDate();
        this.hideCalendar();
    }

    cancel() {
        this.hideCalendar();
        const FirstDate = this.savedDate.FirstDate;
        const SecondDate = this.savedDate.SecondDate;
        this.setState({ Date: {FirstDate, SecondDate} }, this.callUpdateDate.bind(this));
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

    showCalendar() {
        this.setState({ isCalendarVisible: true });
    }

    hideCalendar() {
        this.setState({ isCalendarVisible: false });
    }

    toggleCalendar() {
        const isCalendarVisible = !this.state.isCalendarVisible;
        this.setState({ isCalendarVisible });
    }

    getStyle() {
        let style = { width: 256 };
        if (this.props.isDateRange) {
            style.width = 512;
        }
        return style;
    }

    updateFirstDate(date) {
        const firstDate = date ? date : this.state.Date.FirstDate;
        const secondDate = this.state.Date.SecondDate;
        this.updateDate(firstDate, secondDate, false, { preventHide: true, callUpdateDate: true });
    }

    updateSecondDate(date) {
        const secondDate = date ? date : this.state.Date.SecondDate;
        const firstDate = this.state.Date.FirstDate;
        this.updateDate(firstDate, secondDate, false, { preventHide: true, callUpdateDate: true });
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

        const isCalendarVisible = typeof this.props.isCalendarVisible !== "undefined" ? this.props.isCalendarVisible : this.state.isCalendarVisible;
        const calendarClassName = "calendar-container" + (isCalendarVisible ? " show" : "");

        firstDate = firstDate ? new Date(firstDate) : new Date();
        secondDate = secondDate ? new Date(secondDate) : new Date();

        const showStaticText = !this.props.hasOutsideShowHideControl && this.props.isInputReadOnly;
        const showInputFields = !this.props.hasOutsideShowHideControl && !this.props.isInputReadOnly;
        const showIcon = this.props.showIcon !== false && !this.props.hasOutsideShowHideControl;

        return <div className="dnn-day-picker">
            {showIcon && <div className={"calendar-icon" + (isCalendarVisible ? " active" : "") } onClick={this.toggleCalendar.bind(this) }></div>}
            {!this.props.hasOutsideShowHideControl && <div className="calendar-text" onClick={this.showCalendar.bind(this) }>
                {showStaticText && displayDate}
                {showInputFields && <div style={{ float: "right" }}>
                    <DateInput date={firstDate} onUpdateDate={this.updateFirstDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    {this.props.isDateRange && <div>
                        <span>&nbsp; -&nbsp; </span>
                        <DateInput date={secondDate} onUpdateDate={this.updateSecondDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    </div>}
                </div>}
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
                {showButton && <button role="primary" onClick={this.apply.bind(this) }>{this.props.applyButtonText || "Apply"}</button>}
            </div>
        </div >;
    }
}

DatePicker.propTypes = {
    // Required Props
    date: PropTypes.instanceOf(Date),
    updateDate: PropTypes.func.isRequired,

    // Optional Props
    secondDate: PropTypes.instanceOf(Date),

    // if set to true, it shows 2 calendars
    isDateRange: PropTypes.bool,

    // if set ot to true, it shows time picker 
    hasTimePicker: PropTypes.bool,

    // showInput is true by default. 
    // If showInput is false the controll of showing/hiding the calendar should be performed outside of the component. 
    // In this case isCalendarVisible.bool is Required. Parent Component should handle show/hide logic
    hasOutsideShowHideControl: PropTypes.bool,
    isCalendarVisible: PropTypes.bool,
    controllerClassName: PropTypes.string,

    //if set to true it shows static text insted of input fields
    isInputReadOnly: PropTypes.bool,

    //show/hide an icon
    showIcon: PropTypes.bool,
    applyButtonText: PropTypes.string
};