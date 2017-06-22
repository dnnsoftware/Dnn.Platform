import React, { Component, PropTypes } from "react";
import Switch from "dnn-switch";
import Label from "dnn-label";
import DatePicker from "dnn-date-picker";
import Localization from "../../../localization";

class EnableScheduling extends Component {

    render() {
        const {props} = this;
        return <div  style={{marginTop:"30px"}}>
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
    schedulingEnabled: PropTypes.bool.isRequired,
    onChangeSchedulingEnabled: PropTypes.func.isRequired,
    startDate: PropTypes.date,
    endDate: PropTypes.date,
    onChangeStartDate: PropTypes.func.isRequired,
    onChangeEndDate: PropTypes.func.isRequired
};

export default EnableScheduling;