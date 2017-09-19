import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";

import DayPicker from "../DayPicker/src/DayPicker.jsx";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";


class DropdownDayPicker extends Component  {

    constructor(){
        super();
    }

    componentDidMount() {
        document.addEventListener('click', this.handleClick.bind(this), false);
        this._isMounted = true;
    }


    componentWillUnmount() {
        document.removeEventListener('click', this.handleClick.bind(this), false);
        this._isMounted = false;
    }

     handleClick(e) {
        const {toggleDropdownCalendar, dropdownIsActive} = this.props;
        if (!this._isMounted) { return; }
        if(!ReactDOM.findDOMNode(this).contains(e.target)) {
            dropdownIsActive ? toggleDropdownCalendar(false) : null;
        }
    }

    render(){
        const {dropdownIsActive, onDayClick, applyChanges, startDate, endDate, CalendarIcon, toggleDropdownCalendar} = this.props;
        return(
            <div className="date-picker">
                <GridCell className="calendar-dropdown-container" columnSize={100} style={{padding: "0px 5px"}}>
                    <GridCell className="selected-date" columnSize={90}>
                        <p>Filter by Published Date Range</p>
                    </GridCell>
                    <GridCell columnSize={10}>
                        <div    id="calendar-icon"
                                className="calendar-icon"
                                dangerouslySetInnerHTML={{__html:CalendarIcon}}
                                onClick={()=>toggleDropdownCalendar() }/>
                    </GridCell>
                        <div
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
                        </div>
                    </GridCell>
                </div>
            );
    }

}


export default DropdownDayPicker;

DropdownDayPicker.propTypes = {
    dropdownIsActive: PropTypes.bool.isRequired,
    applyChanges: PropTypes.func.isRequired,
    onDayClick: PropTypes.func.isRequired,
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    CalendarIcon: PropTypes.node.isRequired,
    toggleDropdownCalendar: PropTypes.func.isRequired
};

