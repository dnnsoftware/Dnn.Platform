import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import resx from "../../../resources";
import ProgressBar from "./progressBar";
import "./style.less";

class TranslationProgressBars extends Component {
    constructor() {
        super();
        this.currentTime = Date.now();
    }

    normalizeTime(time) {
        if (time < 10) {
            return "0" + time;
        }
        return time;
    }

    getTime() {
        const time = Date.now() - this.currentTime;
        const seconds = Math.floor(time / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        return `${this.normalizeTime(hours)}:${this.normalizeTime(minutes % 60)}:${this.normalizeTime(seconds % 60)}s`;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        const totalProgressText =
            resx.get("TotalProgress").replace("[number]", props.PrimaryPercent) + " - " +
            resx.get("TotalLanguages").replace("[number]", props.PrimaryTotal);
        const progressText = resx.get("Progress").replace("[number]", props.SecondaryPercent);

        return <div className={"translation-progress-bars" + (props.Error ? " error" : "") }>
            <div className="text">
                <span>{resx.get("TranslationProgressBarText").replace("[number]", props.SecondaryTotal) }</span>
            </div>
            <ProgressBar text={totalProgressText} percentageValue={props.PrimaryPercent}/>
            <ProgressBar text={progressText} rightText={props.CurrentOperationText} percentageValue={props.SecondaryPercent}/>
            <div className="text time">
                {resx.get("ElapsedTime") }
                <span>{this.getTime() }</span>
            </div>
        </div>;
    }
}

TranslationProgressBars.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array,
    languageSettings: PropTypes.object,
    InProgress: PropTypes.bool,
    PrimaryPercent: PropTypes.number,
    PrimaryTotal: PropTypes.number,
    PrimaryValue: PropTypes.number,
    SecondaryPercent: PropTypes.number,
    SecondaryTotal: PropTypes.number,
    SecondaryValue: PropTypes.number,
    TimeEstimated: PropTypes.number,
    CurrentOperationText: PropTypes.array,
    Error: PropTypes.strings
};

function mapStateToProps(state) {

    return {
        languages: state.languages.languageList,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(TranslationProgressBars);