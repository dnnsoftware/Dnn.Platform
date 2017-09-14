import React, {Component, PropTypes} from "react";
import Picker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import "./style.less";


const DayPicker = (props) => {

    const {onDayClick} = props;
    return(
        <div className="dnn-day-picker">
            <Picker {...props} />
        </div>);
};


DayPicker.propTypes = {
    onDayClick: PropTypes.func.isRequired,
    selectedDays: PropTypes.func.isRequired
};

export default DayPicker;

