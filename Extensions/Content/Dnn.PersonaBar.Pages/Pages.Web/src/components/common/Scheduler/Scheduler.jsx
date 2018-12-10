import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "../../../localization";
import styles from "./style.less";
import { Switch, Label, DatePicker } from "@dnnsoftware/dnn-react-common";

class Scheduler extends Component {
    constructor() {
        super();
        this.state = {
            schedulingEnabled: false
        };
    }

    onChangeScheduling(enabled) {
        this.setState({
            schedulingEnabled: enabled
        });
    }

    render() {
        const {schedulingEnabled} = this.state;
        const {startDate, endDate, onChange} = this.props;

        return (
            <div className={styles.scheduler}>
                <Label
                    labelType="inline"
                    tooltipMessage={Localization.get("EnableSchedulingTooltip") }
                    label={Localization.get("EnableScheduling") }
                    />
                <Switch
                    labelHidden={false}
                    onText={Localization.get("On") }
                    offText={Localization.get("Off") }
                    value={schedulingEnabled}
                    onChange={this.onChangeScheduling.bind(this) } />
                <div style={{ clear: "both" }}></div>
                {schedulingEnabled &&
                    <div className="scheduler-date-box">
                        <div className="scheduler-date-row">
                            <Label
                                label={Localization.get("StartDate") } />
                            <DatePicker
                                date={startDate}
                                updateDate={(date) => onChange("startDate", date) }
                                isDateRange={false}
                                hasTimePicker={true}
                                showClearDateButton={false} />
                        </div>
                        <div className="scheduler-date-row">
                            <Label
                                label={Localization.get("EndDate") } />
                            <DatePicker
                                date={endDate}
                                updateDate={(date) => onChange("endDate", date) }
                                isDateRange={false}
                                hasTimePicker={true}
                                showClearDateButton={false} />
                        </div>
                    </div>
                }
            </div>
        );
    }
}

Scheduler.propTypes = {
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    onChange: PropTypes.func.isRequired
};

export default Scheduler;