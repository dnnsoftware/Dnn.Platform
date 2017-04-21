import React, {PropTypes} from "react";
import timeZones from "./timeZones";
import Dropdown from "./Dropdown";

const MAX_LABEL_LENGHT = 20;

function getAbbreviatedLabel(label) {
    return (label && label.length > MAX_LABEL_LENGHT) ? label.substring(0, MAX_LABEL_LENGHT - 3) + "..." : label;
}

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
            abbreviatedLabel={getAbbreviatedLabel(label)}
            onUpdate={onUpdate}
            className="timezone-picker" />
    );
};

TimezonePicker.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    value: PropTypes.string
};

export default TimezonePicker;