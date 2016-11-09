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
import "./style.less";

class LanguagePackPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            makeAllPagesTranslatable: false,
            triedToSubmit: false
        };
    }

    onEnable(event) {
    }

    getDefaultLanguage() {
        const {languages, languageSettings} = this.props;
        if (!languages || !languageSettings) {
            return {};
        }
        return languages.find(l => l.Code === languageSettings.SiteDefaultLanguage);
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let languageSettings = Object.assign({}, props.languageSettings);
        languageSettings[key] = typeof (event) === "object" ? event.target.value : event;
        props.dispatch(LanguagesActions.languageSettingsClientModified(languageSettings));
    }

    /* eslint-disable react/no-danger */
    render() {
        const defaultLanguage = this.getDefaultLanguage();
        const {props, state} = this;
        return (
            <SocialPanelBody
                className="create-language-pack-panel enable-localized-content-panel"
                workSpaceTrayOutside={true}
                workSpaceTray={<div className="siteSettings-back dnn-grid-cell" onClick={props.closePersonaBarPage.bind(this) }>
                    {resx.get("BackToLanguages") }
                </div>}
                workSpaceTrayVisible={true}>
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
                                <img src={defaultLanguage.Icon} />
                            </div>
                            <div className="language-name">{defaultLanguage.EnglishName }</div>
                        </div>
                        {props.languageSettings && <Switch
                            value={props.languageSettings.AllPagesTranslatable}
                            onChange={this.onSettingChange.bind(this, "AllPagesTranslatable") }
                            />}
                        <Label
                            className="float-right"
                            tooltipMessage={resx.get("AllPAgesTranslatable.Help") }
                            label={resx.get("AllPAgesTranslatable") }
                            />
                    </InputGroup>

                    <div className="buttons-box">
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
                    </div>
                </div>
            </SocialPanelBody>
        );
    }
}

LanguagePackPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array,
    languageSettings: PropTypes.obj
};

function mapStateToProps(state) {
    console.log('state:', state);

    return {
        languages: state.languages.languageList,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(LanguagePackPanelBody);