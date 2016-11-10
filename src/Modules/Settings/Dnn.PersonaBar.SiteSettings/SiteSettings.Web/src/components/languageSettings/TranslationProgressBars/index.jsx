import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import SocialPanelBody from "dnn-social-panel-body";
import InputGroup from "dnn-input-group";
import Label from "dnn-label";
import Button from "dnn-button";
import Switch from "dnn-switch";
import resx from "../../../resources";
import ProgressBar from "./progressBar";
import "./style.less";

class TranslationProgressBars extends Component {
    constructor() {
        super();
        this.state = {
            totalProgress: 0,
            totalLanguages: 2,
            currentLanguage: "",
            currentPage: "",
            progress: 30,
            totalPages: 35,
            elapsedTime: ""
        };
        this.currentTime = Date.now();
        this.update();
    }

    normalizeTime(time) {
        if (time < 10) {
            return "0" + time;
        }
        return time;
    }

    getTime() {
        const time = Date.now() - this.currentTime;
        const seconds = this.normalizeTime(Math.floor(time/1000));
        const minutes = this.normalizeTime(Math.floor(seconds/60));
        const hours = this.normalizeTime(Math.floor(minutes/60));
        return `${hours}:${minutes}:${seconds}s`;
    }


    updateStates() {
        let {progress, totalProgress} = this.state;
        totalProgress+=5;
        progress+=5;
        if (progress > 100) {
            progress=100;
        }
        if (totalProgress > 100) {
            totalProgress = 100;
        }
        this.setState({progress, totalProgress});
    }

    update() {
        this.updateStates();
        if (this.state.progress >= 100 && this.state.totalProgress >= 100) {
            return;
        }
        setTimeout(this.update.bind(this), 500);
    }

    /* eslint-disable react/no-danger */
    render() {
        const {state, props} = this;
        const totalProgressText = 
            resx.get("TotalProgress").replace("[number]", state.totalProgress) + " - " +
            resx.get("TotalLanguages").replace("[number]", state.totalLanguages);
        const progressText = resx.get("Progress").replace("[number]", state.progress);

        return <div className="translation-progress-bars">
            <div className="text">
                <span>{resx.get("TranslationProgressBarText").replace("[number]", state.totalPages)}</span>
            </div>
            <ProgressBar text={totalProgressText} procentageValue={state.totalProgress}/>
            <ProgressBar text={progressText} procentageValue={state.progress}/>
            <div className="text time">
                {resx.get("ElapsedTime")}
                <span>{this.getTime()}</span>
            </div>
        </div>;
    }
}

TranslationProgressBars.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array,
    languageSettings: PropTypes.obj
};

function mapStateToProps(state) {

    return {
        languages: state.languages.languageList,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(TranslationProgressBars);