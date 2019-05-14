import React, { Component } from "react";
import PropTypes from "prop-types";
import "./progressBar.less";

class ProgressBar extends Component {
    constructor(props) {
        super(props);
        this.percentageValue = this.props.percentageValue;
    }
    /* eslint-disable react/no-danger */
    render() {

        let transition = "1s linear";
        if (this.percentageValue > this.props.percentageValue) {
            transition = "0s linear";
        }
        this.percentageValue = this.props.percentageValue;
        const {props} = this;
        let style = { width: 0 , transition};
        if (props.percentageValue) {
            style.width = props.percentageValue + "%";
        } else if (props.currentValue && props.totalValue) {
            style.width = ((props.currentValue * 100) / props.totalValue) + "%";
        }
        return <div className="dnn-progress-bar">
            <span className="float-left">{props.text}</span>
            <span className="float-right">{props.rightText}</span>
            <div className="progress-bar-container">
                <div className="progress-indicator-bar" style={style}></div>
            </div>
        </div>;
    }
}

ProgressBar.propTypes = {
    currentValue: PropTypes.number,
    totalValue: PropTypes.number,
    percentageValue: PropTypes.number,
    text: PropTypes.string,
    rightText: PropTypes.string
};

export default ProgressBar;