import React, { Component } from "react";
import PropTypes from "prop-types";
import { Switch, Label, DatePicker } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";

class EnableScheduling extends Component {

    render() {
        const {props} = this;
        return <div>
            <Label
                labelType="inline"
                tooltipMessage={Localization.get("EnableSchedulingTooltip") }
                label={Localization.get("EnableScheduling") }
                />
            <Switch
                labelHidden={false}
                onText={Localization.get("On") }
                offText={Localization.get("Off") }
                value={props.schedulingEnabled}
                onChange={props.onChangeSchedulingEnabled} />
            <div style={{ clear: "both" }}></div>
            {props.schedulingEnabled &&
                <div className="scheduler-date-box">
                    <div className="scheduler-date-row">
                        <Label
                            label={Localization.get("StartDate") } />
                        <DatePicker
                            date={props.startDate}
                            updateDate={props.onChangeStartDate}
                            calendarPosition={"top"}
                            isDateRange={false}
                            hasTimePicker={true}
                            showClearDateButton={false} />
                    </div>
                    <div className="scheduler-date-row">
                        <Label
                            label={Localization.get("EndDate") } />
                        <DatePicker
                            date={props.endDate}
                            updateDate={props.onChangeEndDate}
                            calendarPosition={"top"}
                            isDateRange={false}
                            hasTimePicker={true}
                            showClearDateButton={false} />
                    </div>
                    <div style={{ clear: "both" }}></div>
                </div>
            }
        </div>;
    }
}

EnableScheduling.propTypes = {
    schedulingEnabled: PropTypes.bool,
    onChangeSchedulingEnabled: PropTypes.func.isRequired,
    startDate: PropTypes.instanceOf(Date),
    endDate: PropTypes.instanceOf(Date),
    onChangeStartDate: PropTypes.func.isRequired,
    onChangeEndDate: PropTypes.func.isRequired
};

export default EnableScheduling;