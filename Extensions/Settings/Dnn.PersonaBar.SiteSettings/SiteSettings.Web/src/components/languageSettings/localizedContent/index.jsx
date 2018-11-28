import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import InputGroup from "dnn-input-group";
import Label from "dnn-label";
import Button from "dnn-button";
import Switch from "dnn-switch";
import resx from "../../../resources";
import util from "../../../utils";
import TranslationProgressBars from "../TranslationProgressBars";
import "./style.less";

class LocalizedContent extends Component {
    constructor() {
        super();
        this.state = {
            triedToSubmit: false,
            allPagesTranslatable: false,
            InProgress: false,
            showProgressBars: false
        };
        this.counter = 0;
        this.getProgressData = this.getProgressData.bind(this);
    }

    onEnable() {
        const {props, state} = this;
        props.dispatch(LanguagesActions.enableLocalizedContent({ portalId: props.portalId, translatePages: state.allPagesTranslatable }, (data) => {
            this.setState(data);
            const newState = Object.assign(data, { showProgressBars: true });
            this.setState(newState);
            this.getProgressData();
        }, (error) => {
            const errorMessage = JSON.parse(error.responseText);
            util.utilities.notifyError(errorMessage.Message);
        }));
    }

    onUpdateSettings() {
        const {props} = this;
        props.dispatch(LanguagesActions.getLanguageSettings(props.languageSettings.PortalId, props.languageSettings.CultureCode));
    }

    getProgressData() {
        const {props} = this;
        props.dispatch(LanguagesActions.getLocalizationProgress((data) => {
            this.setState(data);
            if (data.InProgress && !data.Error) {
                return setTimeout(this.getProgressData, 1000);
            }
            if (data.Error) {
                return;
            }
            this.doneProgress();
        }));
    }

    doneProgress() {
        if (this.props.isOpen) {
            this.props.closePersonaBarPage();
            this.onUpdateSettings();
        }
        this.setState({ showProgressBars: false });
    }

    getDefaultLanguage() {
        const {languages, languageSettings} = this.props;
        if (!languages || !languageSettings) {
            return;
        }
        return languages.find(l => l.Code === languageSettings.SiteDefaultLanguage);
    }

    onChange(event) {
        const allPagesTranslatable = typeof (event) === "object" ? event.target.value : event;
        this.setState({ allPagesTranslatable });
    }

    /* eslint-disable react/no-danger */
    render() {
        
        const defaultLanguage = this.getDefaultLanguage() || {};
        const {props, state} = this;
        return (
            <PersonaBarPageBody
                className="create-language-pack-panel enable-localized-content-panel"
                backToLinkProps={{
                    text: resx.get("BackToLanguages"),
                    onClick: props.closePersonaBarPage
                }}>
                <div className="languagePack-wrapper">
                    <InputGroup>
                        <div className="help-text-with-background">
                            <p>{resx.get("EnableLocalizedContentHelpText") }</p>
                            <p>{resx.get("EnableLocalizedContentClickCancel") }</p>
                        </div>
                    </InputGroup>

                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("CurrentSiteDefault.Help") }
                            label={resx.get("CurrentSiteDefault") }
                        />
                        <div>
                            <div className="language-flag">
                                {!!defaultLanguage.Icon && <img src={defaultLanguage.Icon} alt={defaultLanguage.EnglishName} />}
                            </div>
                            <div className="language-name">{defaultLanguage.EnglishName}</div>
                        </div>
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.allPagesTranslatable}
                            onChange={this.onChange.bind(this) }
                        />
                        <Label
                            className="float-right"
                            tooltipMessage={resx.get("AllPagesTranslatable.Help") }
                            label={resx.get("AllPagesTranslatable") }
                        />
                    </InputGroup>

                    {!state.showProgressBars && <div className="buttons-box">
                        <Button
                            type="secondary"
                            onClick={props.closePersonaBarPage.bind(this) }>
                            {resx.get("Cancel") }
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onEnable.bind(this) }>
                            {resx.get("EnableLocalizedContent") }
                        </Button>
                    </div>}
                    {this.state.showProgressBars && <TranslationProgressBars
                        InProgress={this.state.InProgress}
                        PrimaryPercent={this.state.PrimaryPercent}
                        PrimaryTotal={this.state.PrimaryTotal}
                        PrimaryValue={this.state.PrimaryValue}
                        SecondaryPercent={this.state.SecondaryPercent}
                        SecondaryTotal={this.state.SecondaryTotal}
                        SecondaryValue={this.state.SecondaryValue}
                        TimeEstimated={this.state.TimeEstimated}
                        Error={this.state.Error}
                        CurrentOperationText={this.state.CurrentOperationText}
                        closePersonaBarPage={props.closePersonaBarPage}/>
                    }
                </div>
            </PersonaBarPageBody>
        );
    }
}

LocalizedContent.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array,
    isOpen: PropTypes.bool,
    languageSettings: PropTypes.obj,
    portalId: PropTypes.number
};

function mapStateToProps(state) {

    return {
        languages: state.languages.languageList,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(LocalizedContent);