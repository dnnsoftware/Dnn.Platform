import React, {Component} from "react";
import PropTypes from "prop-types";
import DayPicker from "../DayPicker/DayPicker.jsx";
import { Button, GridCell } from "@dnnsoftware/dnn-react-common";
import "./styles.less";
import Localization from "../../localization";

class DropdownDayPicker extends Component  {

    constructor() {
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
        if (!this.node.contains(e.target)) {
            dropdownIsActive ? toggleDropdownCalendar(false) : null;
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {dropdownIsActive, onDayClick, applyChanges, clearChanges, startDate, endDate, CalendarIcon, toggleDropdownCalendar} = this.props;

        return (
            <div className="date-picker" ref={node => this.node = node}>
                <GridCell className="calendar-dropdown-container" columnSize={100} style={{padding: "0px 5px"}}>
                    <GridCell className="selected-date" columnSize={90}>
                        <p>{this.props.label}</p>
                    </GridCell>
                    <GridCell columnSize={10}>
                        <div    id="calendar-icon"
                                className="calendar-icon"
                                dangerouslySetInnerHTML={{__html:CalendarIcon}}
                                onClick={()=>toggleDropdownCalendar() }/>
                    </GridCell>
                        <div
                            id="calendar-dropdown"
                            className={dropdownIsActive ? "calendar-dropdown expand-down" : `calendar-dropdown ${dropdownIsActive !== null ? 'expand-up' : ''} ` } >
                            <GridCell columnSize={100} style={{padding:"20px"}}>
                                <GridCell columnSize={50}  className="calendar">
                                        <DayPicker
                                            id="try-this"
                                            month={startDate}
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
                                    <div className="calendar-dropdown-action-buttons">
                                        <Button type="secondary" style={{"margin-right":"5px"}} onClick={()=>clearChanges()}>{Localization.get("Clear")}</Button>
                                        <Button type="primary" onClick={()=>applyChanges()}>{Localization.get("Apply")}</Button>
                                    </div>
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
    clearChanges: PropTypes.func.isRequired,
    onDayClick: PropTypes.func.isRequired,
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    CalendarIcon: PropTypes.node.isRequired,
    toggleDropdownCalendar: PropTypes.func.isRequired,
    label: PropTypes.string.isRequired
};

