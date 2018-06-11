import React, { PropTypes } from "react";
import Picker from "react-day-picker";
import "./style.less";


const DayPicker = (newProps) => {
    return (
        <div className="dnn-day-picker">
            <Picker {...newProps} />
        </div>);
};
DayPicker.PropTypes = {
    onDayClick: PropTypes.func.isRequired,
    month: PropTypes.instanceOf(Date),
    selectedDays: PropTypes.instanceOf(Date),
    fromMonth: PropTypes.instanceOf(Date)
};

export default DayPicker;