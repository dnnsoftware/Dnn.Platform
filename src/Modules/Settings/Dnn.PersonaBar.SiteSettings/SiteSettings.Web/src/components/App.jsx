import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import {
    visiblePanel as VisiblePanelActions
} from "../actions";
import Body from "./body";
import PersonaBarPage from "dnn-persona-bar-page";
import HtmlEditorManager from "./moreSettings/htmlEditorManager";
import LanguageVerifier from "./languageSettings/languageVerifier";
import LanguagePack from "./languageSettings/languagePack";
import LocalizedContent from "./languageSettings/localizedContent";
import TranslatePageContent from "./translatePageContent/translatePageContent";
import EditLanguagePanel from "./editLanguagePanel";
import utilities from "utils";
import resx from "../resources";

const Pages = {
    Default: 0,
    HtmlEditorManager: 1,
    LanguageVerifier: 2,
    LanguageEditor: 3,
    LanguagePack: 4,
    LocalizedContent: 5,
    TranslatePageContent: 6
};

class App extends Component {
    constructor() {
        super();
        this.state = {
            portalId: utilities.settings.PortalID,
            referrer: utilities.settings.referrer,
            referrerText: utilities.settings.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, utilities.settings.backToReferrerFunc),
            bodyShowing: true
        };
        this.changePortalId = this.changePortalId.bind(this);
    }

    backToReferrer(callback) {
        if (typeof callback === "function") {
            callback();
        }
        setTimeout(() => {
            this.setState({
                referrer: "",
                referrerText: "",
                backToReferrerFunc: null
            });
        }, 1500);
    }

    changePortalId(portalId) {
        this.setState({
            bodyShowing: false
        }, () => {
            this.setState({
                bodyShowing: true,
                portalId
            });
        });
    }

    updateReferrerInfo(event) {
        this.setState({
            referrer: event.referrer,
            referrerText: event.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, event.backToReferrerFunc)
        });
    }

    componentWillMount() {
        // // Listen for the event.
        document.addEventListener("portalIdChanged", (e) => {
            if (this.state.portalId !== e.portalId) {
                this.changePortalId(e.portalId);
            }
            this.updateReferrerInfo(e);
        }, false);
    }

    openPersonaBarPage(pageNumber) {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(pageNumber));
    }

    closePersonaBarPage() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(Pages.Default));
    }

    render() {
        const {props, state} = this;
        return (
            <div className="siteSettings-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={resx.get("nav_SiteSettings")}>
                    </PersonaBarPageHeader>
                    <Body
                        portalId={state.portalId}
                        showing={state.bodyShowing}
                        referrer={state.referrer}
                        referrerText={state.referrerText}
                        backToReferrerFunc={state.backToReferrerFunc}
                        openLanguagePack={this.openPersonaBarPage.bind(this, Pages.LanguagePack)}
                        openLanguageVerifier={this.openPersonaBarPage.bind(this, Pages.LanguageVerifier)}
                        openHtmlEditorManager={this.openPersonaBarPage.bind(this, Pages.HtmlEditorManager)}
                        openLocalizedContent={this.openPersonaBarPage.bind(this, Pages.LocalizedContent)}
                        cultureCode={props.cultureCode}
                        />
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.HtmlEditorManager}>
                    <PersonaBarPageHeader title={resx.get("nav_SiteSettings")}>
                    </PersonaBarPageHeader>
                    <HtmlEditorManager portalId={state.portalId} closeHtmlEditorManager={this.closePersonaBarPage.bind(this)} />
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguageVerifier}>
                    <PersonaBarPageHeader title={resx.get("ResourceFileVerifier")}>
                    </PersonaBarPageHeader>
                    <LanguageVerifier portalId={props.portalId} closeLanguageVerifier={this.closePersonaBarPage.bind(this)} />
                </PersonaBarPage>

                {props.selectedPage === Pages.LanguageEditor && <PersonaBarPage isOpen={props.selectedPage === Pages.LanguageEditor}>
                    <PersonaBarPageHeader title={resx.get("LanguageEditor.Header")} />
                    <EditLanguagePanel portalId={state.portalId} backToSiteSettings={this.closePersonaBarPage.bind(this)} />
                </PersonaBarPage>}

                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguagePack}>
                    <PersonaBarPageHeader title={resx.get("CreateLanguagePack")} />
                    <LanguagePack closeLanguagePack={this.closePersonaBarPage.bind(this)} />
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.LocalizedContent}>
                    <PersonaBarPageHeader title={resx.get("EnableLocalizedContent")} />
                    <LocalizedContent portalId={state.portalId} closePersonaBarPage={this.closePersonaBarPage.bind(this)} isOpen={props.selectedPage === Pages.LocalizedContent} />
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.TranslatePageContent}>
                    <PersonaBarPageHeader title={resx.get("TranslatePageContent")} />
                    {props.selectedPage === Pages.TranslatePageContent &&
                        <TranslatePageContent cultureCode={props.code} closePersonaBarPage={this.closePersonaBarPage.bind(this)} portalId={state.portalId} />
                    }
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