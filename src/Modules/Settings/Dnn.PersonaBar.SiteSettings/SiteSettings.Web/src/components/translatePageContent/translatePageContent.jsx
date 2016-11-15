import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../resources";
import { Scrollbars } from "react-custom-scrollbars";
import SocialPanelBody from "dnn-social-panel-body";
import TranslationProgressBars from "../languageSettings/TranslationProgressBars";
import Button from "dnn-button";
import {
    languages as LanguagesActions
} from "actions";


class TranslatePageContent extends Component {
    constructor() {
        super();
        this.state = {
            pageList: []
        };
        this.getProgressData = this.getProgressData.bind(this);
    }

    componentWillMount() {
        const {props, state} = this;
        const cultureCode = props.languageBeingEdited.Code;

        props.dispatch(LanguagesActions.getPageList(cultureCode, (data) => {
            console.log('DATA PAGES:', data);
            this.setState({ pageList: data });
        }));
        this.getProgressData();
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
                <a className="float-rigth">{resx.get("EditPageSettings") }</a>
                <a className="float-rigth" target="_blank" href={page.ViewUrl}>{resx.get("ViewPage") }</a>
            </div>;
        });
    }

    onSave() {

    }

    render() {
        const {props, state} = this;
        return <SocialPanelBody
            className="create-language-pack-panel enable-localized-content-panel"
            workSpaceTrayOutside={true}
            workSpaceTray={<div className="siteSettings-back dnn-grid-cell" onClick={props.closePersonaBarPage }>
                {resx.get("BackToLanguages") }
            </div>}
            workSpaceTrayVisible={true}>
            <div className="languagePack-wrapper">

                <div className="language-settings-page-list">
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
                <div className="buttons-box">
                    <Button
                        type="secondary"
                        onClick={props.closePersonaBarPage.bind(this) }>
                        {resx.get("Cancel") }
                    </Button>
                    <Button
                        type="primary"
                        onClick={this.onSave.bind(this) }>
                        {resx.get("EnableLocalizedContent") }
                    </Button>
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
    closePersonaBarPage: PropTypes.func
};

function mapStateToProps(state) {
    return {
        pageList: state.languages.pageList,
        languageBeingEdited: state.languageEditor.languageBeingEdited
    };
}

export default connect(mapStateToProps)(TranslatePageContent);