import React, {PropTypes} from "react";
import timeZones from "./timeZones";
import Dropdown from "./Dropdown";

const TimezonePicker = ({value, onUpdate}) => 
    <Dropdown 
        options={timeZones}
        value={value}
        onUpdate={onUpdate}
        className="timezone-picker" />;

TimezonePicker.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    value: PropTypes.string
};

export default TimezonePicker;