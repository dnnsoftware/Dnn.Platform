import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../resources";
import { Scrollbars } from "react-custom-scrollbars";
import Tooltip from "dnn-tooltip";
import InputGroup from "dnn-input-group";
import Switch from "dnn-switch";
import Label from "dnn-label";
import SocialPanelBody from "dnn-social-panel-body";
import TranslationProgressBars from "../languageSettings/TranslationProgressBars";
import Button from "dnn-button";
import {
    languages as LanguagesActions,
    siteInfo as SiteInfoActions
} from "actions";


class TranslatePageContent extends Component {
    constructor() {
        super();
        this.state = {
            pageList: [],
            basicSettings: null,
            languageBeingEdited: null
        };
        this.getProgressData = this.getProgressData.bind(this);
    }

    componentWillMount() {
        const {props, state} = this;
        this.setState({ languageBeingEdited: props.languageBeingEdited });
        this.getPageList();
        this.getBasicSettings();
        this.getProgressData();
    }

    getPageList() {
        const {props, state} = this;
        const cultureCode = props.languageBeingEdited.Code;
        props.dispatch(LanguagesActions.getPageList(cultureCode, (data) => {
            this.setState({ pageList: data });
        }));
    }

    getBasicSettings() {
        const {props, state} = this;
        props.dispatch(SiteInfoActions.getPortalSettings(props.portalId, props.cultureCode, (data) => {
            this.setState({
                basicSettings: Object.assign(data.Settings)
            });
        }));

    }

    getProgressData() {
        const {props, state} = this;
        props.dispatch(LanguagesActions.getLocalizationProgress((data) => {
            this.setState(data);
            if (data.InProgress && !data.Error) {
                return setTimeout(this.getProgressData, 500);
            }
            if (data.Error) {
                return;
            }
            this.doneProgress();
        }));
    }

    doneProgress() { }

    renderPageList() {
        const {pageList} = this.state;
        if (!pageList) {
            return;
        }
        return pageList.map((page) => {
            return <div className="page-list-item">
                <span>{page.PageName}</span>
                <a className="float-right">{resx.get("EditPageSettings") }</a>
                <a className="float-right" target="_blank" href={page.ViewUrl}>{resx.get("ViewPage") }</a>
            </div>;
        });
    }

    onSave() {

    }

    onMarkAllPagesAsTranslated() {

    }

    onEraseAllLocalizedPages() {
        const {props, state} = this;
        const cultureCode = props.languageBeingEdited.Code;
        props.dispatch(LanguagesActions.deleteLanguagePages(cultureCode, (data) => {
            console.log('ERASE DATA: ', data);
            
            this.getPageList();
        }));
    }

    onPublishTranslatedPages(enable=true) {
        const {props, state} = this;
        const cultureCode = props.languageBeingEdited.Code;
        props.dispatch(LanguagesActions.publishAllPages({cultureCode, enable}, (data) => {
            console.log('PUBLISH DATA: ', data);
        }));
    }

    onCancel() {
        this.props.closePersonaBarPage();
    }

    render() {

        const {props, state} = this;
        const language = state.languageBeingEdited;
        console.log('LANGUAGE:', language);
        return <SocialPanelBody
            className="translate-page-content"
            workSpaceTrayOutside={true}
            workSpaceTray={<div className="siteSettings-back dnn-grid-cell" onClick={props.closePersonaBarPage }>
                {resx.get("BackToLanguages") }
            </div>}
            workSpaceTrayVisible={true}>

            <div className="language-settings-page-list">
                {language && <div className="top-block">
                    <div className="language-block">
                        <img className="float-left" src={language.Icon} />
                        <span >{language.NativeName}</span>
                        {state.basicSettings && <span className="float-right">{state.basicSettings.PortalName}</span>}
                    </div>
                    <div className="button-block">
                        <Button
                            type="secondary"
                            onClick={this.onMarkAllPagesAsTranslated.bind(this) }>
                            {resx.get("MarkAllPagesAsTranslated") }
                        </Button>
                        <Button
                            type="secondary"
                            onClick={this.onEraseAllLocalizedPages.bind(this) }>
                            {resx.get("EraseAllLocalizedPages") }
                        </Button>
                        <Button
                            type="primary"
                            className="float-right"
                            onClick={this.onPublishTranslatedPages.bind(this, true) }>
                            {resx.get("PublishTranslatedPages") }
                        </Button>
                        <Button
                            type="secondary"
                            className="float-right"
                            onClick={this.onPublishTranslatedPages.bind(this, false) }>
                            {resx.get("UnpublishTranslatedPages") }
                        </Button>
                    </div>
                </div>}

                <div className="list-header">
                    <span>{resx.get("PagesToTranslate") } <span>{language.LocalizablePages}</span></span>
                    <span className="float-right"><em>+</em>{resx.get("AddAllUnlocalizedPages") }</span>
                </div>

                {this.state.InProgress && <TranslationProgressBars
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
                    />}
                <div className="page-list">
                    <Scrollbars className="scrollArea content-vertical"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={500}>
                        {this.renderPageList() }
                    </Scrollbars>
                </div>
            </div>
        </SocialPanelBody>;
    }
}

TranslatePageContent.propTypes = {
    dispatch: PropTypes.func.isRequired,
    pageList: PropTypes.array,
    cultureCode: PropTypes.string,
    onSelectChange: PropTypes.func,
    portalId: PropTypes.number,
    closePersonaBarPage: PropTypes.func
};

function mapStateToProps(state) {
    return {
        pageList: state.languages.pageList,
        languageBeingEdited: state.languageEditor.languageBeingEdited
    };
}

export default connect(mapStateToProps)(TranslatePageContent);