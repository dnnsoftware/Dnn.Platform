import React, {PropTypes} from "react";
import Dropdown from "./Dropdown";

let timeArray = [];
for (let i = 0; i < 24; i++) {
    const hour = getHour(i);
    for (let j = 0; j < 2; j++) {
        timeArray.push(hour + ":" + (j ? "30" : "00") + " " + (i > 11 ? "PM" : "AM"));
    }
}

timeArray = timeArray.map(t => {return {label: t, value: t};});

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

const TimePicker = ({time, updateTime, className}) => 
    <Dropdown 
        options={timeArray}
        label={time}
        onUpdate={updateTime}
        className={"time-picker " + (className || "")} />;

TimePicker.propTypes = {
    updateTime: PropTypes.func.isRequired,
    time: PropTypes.string,
    className: PropTypes.string
};

export default TimePicker;