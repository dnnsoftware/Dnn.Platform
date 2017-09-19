import React, {Component, PropTypes} from "react";

import DayPicker from "../DayPicker/src/DayPicker.jsx";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";


class DropdownDayPicker extends Component  {

    constructor(){
        super();
    }

    render(){
        const {dropdownIsActive, onDayClick, applyChanges, startDate, endDate} = this.props;
        return(<div
                id="calendar-dropdown"
                className={dropdownIsActive ? "calendar-dropdown expand-down" : `calendar-dropdown ${dropdownIsActive != null ? 'expand-up' : ''} ` } >
                <GridCell columnSize={100} style={{padding:"20px"}}>
                    <GridCell columnSize={50}  className="calendar">
                            <DayPicker
                            id="try-this"
                            selectedDays={startDate}
                            onDayClick={(data) => onDayClick(data, false) }/>
                    </GridCell>
                    <GridCell columnSize={50} className="calendar">
                            <DayPicker
                            month={endDate}
                            selectedDays={endDate}
                            fromMonth={startDate}
                            onDayClick={(data) => onDayClick(data, true) }/>
                    </GridCell>
                    <GridCell columnSize={100}>
                        <Button type="primary" onClick={()=>applyChanges()}>Apply</Button>
                    </GridCell>
                </GridCell>
            </div>);
    }

}


export default DropdownDayPicker;

DropdownDayPicker.propTypes = {
    dropdownIsActive: PropTypes.bool.isRequired,
    applyChanges: PropTypes.func.isRequired,
    onDayClick: PropTypes.func.isRequired,
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired
};

