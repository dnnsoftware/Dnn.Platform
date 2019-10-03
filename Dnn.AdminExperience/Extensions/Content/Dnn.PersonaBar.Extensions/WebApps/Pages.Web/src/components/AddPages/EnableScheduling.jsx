import React, { Component } from "react";
import PropTypes from "prop-types";
import Scheduler from "../common/Scheduler/Scheduler";

class EnableScheduling extends Component {

    render() {
        const {props} = this;
        return <div className="input-group">
            <Scheduler
                startDate={props.startDate}
                endDate={props.endDate}
                onChange={(key, value) => props.onChangeValue(key, value)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

EnableScheduling.propTypes = {
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    onChangeValue: PropTypes.func.isRequired
};

export default EnableScheduling;