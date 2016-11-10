import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import {
    visiblePanel as VisiblePanelActions
} from "../actions";
import Body from "./body";
import PersonaBarPage from "dnn-persona-bar-page";
import HtmlEditorManager from "./moreSettings/htmlEditorManager";
import LanguageVerifier from "./languageSettings/languageVerifier";
import LanguagePack from "./languageSettings/languagePack";
import LocalizedContent from "./languageSettings/localizedContent";
import EditLanguagePanel from "./editLanguagePanel";
import resx from "../resources";
require('es6-object-assign').polyfill();
require('array.prototype.find').shim();
require('array.prototype.findindex').shim();

const Pages = {
    Default: 0,
    HtmlEditorManager: 1,
    LanguageVerifier: 2,
    LanguageEditor: 3,
    LanguagePack: 4,
    LocalizedContent: 5
};

class App extends Component {

    openPersonaBarPage(pageNumber) {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(pageNumber));
    }

    closePersonaBarPage() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(Pages.Default));
    }

    render() {
        const {props} = this;
        return (
            <div className="siteSettings-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <SocialPanelHeader title={resx.get("nav_SiteSettings") }>
                    </SocialPanelHeader>
                    <Body
                        portalId={props.portalId}
                        openLanguagePack={this.openPersonaBarPage.bind(this, Pages.LanguagePack) }
                        openLanguageVerifier={this.openPersonaBarPage.bind(this, Pages.LanguageVerifier) }
                        openHtmlEditorManager={this.openPersonaBarPage.bind(this, Pages.HtmlEditorManager) }
                        openLocalizedContent={this.openPersonaBarPage.bind(this, Pages.LocalizedContent) }
                        cultureCode={props.cultureCode}
                        />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === Pages.HtmlEditorManager}>
                    <SocialPanelHeader title={resx.get("nav_SiteSettings") }>
                    </SocialPanelHeader>
                    <HtmlEditorManager portalId={props.portalId} closeHtmlEditorManager={this.closePersonaBarPage.bind(this) } />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguageVerifier}>
                    <SocialPanelHeader title={resx.get("ResourceFileVerifier") }>
                    </SocialPanelHeader>
                    <LanguageVerifier portalId={props.portalId} closeLanguageVerifier={this.closePersonaBarPage.bind(this) } />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguageEditor}>
                    <SocialPanelHeader title={resx.get("LanguageEditor.Header") }/>
                    <EditLanguagePanel backToSiteSettings={this.closePersonaBarPage.bind(this) } />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguagePack}>
                    <SocialPanelHeader title={resx.get("CreateLanguagePack") }/>
                    <LanguagePack closeLanguagePack={this.closePersonaBarPage.bind(this) } />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === Pages.LocalizedContent}>
                    <SocialPanelHeader title={resx.get("EnableLocalizedContent") }/>
                    {props.selectedPage === Pages.LocalizedContent && <LocalizedContent closePersonaBarPage={this.closePersonaBarPage.bind(this) } />}
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};


function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);