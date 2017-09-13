import React, {Component, PropTypes} from "react";
import Picker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import "./style.less";


const DayPicker = (props) => {
 
    const {onDayClick} = props;
    return(
        <div className="dnn-day-picker">
            <Picker onDayClick={onDayClick }/>

        </div>);
};


DayPicker.PropTypes = {
    onDayClick: PropTypes.func.isRequired
};

export default DayPicker;

