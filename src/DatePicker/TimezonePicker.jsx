import React, {PropTypes} from "react";
import timeZones from "./timeZones";
import Dropdown from "./Dropdown";

function getDropdownLabel(value) {
    const currentOption = value ? timeZones.find(o => o.value === value) : value;
    return currentOption ? currentOption.label : "";
}

const TimezonePicker = ({value, onUpdate}) => {
    const label = getDropdownLabel(value);
    return (
        <Dropdown 
            options={timeZones}
            label={label}
            onUpdate={onUpdate}
            className="timezone-picker" />
    );
};

TimezonePicker.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    value: PropTypes.string
};

export default TimezonePicker;