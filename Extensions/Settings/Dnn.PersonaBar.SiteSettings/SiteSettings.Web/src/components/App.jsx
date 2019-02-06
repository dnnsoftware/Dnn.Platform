import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPageHeader, PersonaBarPage} from "@dnnsoftware/dnn-react-common";
import {
    visiblePanel as VisiblePanelActions,
    siteInfo
} from "../actions";
import Body from "./body";
import HtmlEditorManager from "./moreSettings/htmlEditorManager";
import LanguageVerifier from "./languageSettings/languageVerifier";
import LanguagePack from "./languageSettings/languagePack";
import LocalizedContent from "./languageSettings/localizedContent";
import TranslatePageContent from "./translatePageContent/translatePageContent";
import EditLanguagePanel from "./editLanguagePanel";
import SiteLanguageSelector from "./siteLanguageSelector";
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
            portalId: utilities.settings.portalId,
            cultureCode: utilities.settings.cultureCode,
            referrer: utilities.settings.referrer,
            referrerText: utilities.settings.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, utilities.settings.backToReferrerFunc),
            bodyShowing: true
        };
        this.changePortalId = this.changePortalId.bind(this);
        this.changeCultureCode = this.changeCultureCode.bind(this);
        this.changePortalIdCultureCode = this.changePortalIdCultureCode.bind(this);
    }

    backToReferrer(callback) {
        if (typeof callback === "function") {
            callback();
        }
        this.setState({
            referrer: "",
            referrerText: "",
            backToReferrerFunc: null
        });
    }

    changePortalId(portalId) {
        const { props } = this;
        if (portalId === undefined) return;
        this.setState({
            bodyShowing: true,
            portalId
        }, () => {
            props.dispatch(siteInfo.updatePortalId(portalId));
        });
    }

    changeCultureCode(cultureCode) {
        if (cultureCode === undefined) return;
        this.setState({
            bodyShowing: false
        }, () => {
            this.setState({
                bodyShowing: true,
                cultureCode
            });
        });
    }

    changePortalIdCultureCode(portalId, cultureCode) {
        if (portalId === undefined && cultureCode === undefined) return;
        else if (portalId === undefined) {
            this.changeCultureCode(cultureCode);
        }
        else if (cultureCode === undefined) {
            this.changePortalId(portalId);
        }
        else {
            this.setState({
                bodyShowing: false
            }, () => {
                this.setState({
                    bodyShowing: true,
                    portalId,
                    cultureCode
                });
            });
        }
    }

    updateReferrerInfo(event) {
        this.setState({
            referrer: event.referrer,
            referrerText: event.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, event.backToReferrerFunc)
        });
    }

    componentDidMount() {
        const {state, props} = this;

        // // Listen for the event.
        document.addEventListener("portalIdChanged", (e) => {
            this.changePortalId(e.portalId);
            this.updateReferrerInfo(e);
        }, false);

        document.addEventListener("cultureCodeChanged", (e) => {
            this.changeCultureCode(e.cultureCode);
            this.updateReferrerInfo(e);
        }, false);

        document.addEventListener("portalIdCultureCodeChanged", (e) => {
            this.changePortalIdCultureCode(e.portalId, e.cultureCode);
            this.updateReferrerInfo(e);
        }, false);

        if (state.portalId !== props.portalId && state.cultureCode !== props.cultureCode) {
            this.changePortalIdCultureCode(props.portalId, props.cultureCode);
        }
        else if (state.portalId !== props.portalId) {
            this.changePortalId(props.portalId);
        }
        else if (state.cultureCode !== props.cultureCode) {
            this.changeCultureCode(props.cultureCode);
        }
    }

    componentDidUpdate() {
        const {props, state} = this;
        if (state.portalId !== props.portalId && state.cultureCode !== props.cultureCode) {
            this.changePortalIdCultureCode(props.portalId, props.cultureCode);
        }
        else if (state.portalId !== props.portalId) {
            this.changePortalId(props.portalId);
        }
        else if (state.cultureCode !== props.cultureCode) {
            this.changeCultureCode(props.cultureCode);
        }
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
                <SiteLanguageSelector
                    portalId={state.portalId}
                    cultureCode={state.cultureCode}
                    referrer={state.referrer}
                    referrerText={state.referrerText}
                    backToReferrerFunc={state.backToReferrerFunc}
                />
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
                        cultureCode={state.cultureCode}
                    />
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.HtmlEditorManager}>
                    <PersonaBarPageHeader title={resx.get("nav_SiteSettings")}>
                    </PersonaBarPageHeader>
                    {props.selectedPage === Pages.HtmlEditorManager &&
                        <HtmlEditorManager portalId={state.portalId} closeHtmlEditorManager={this.closePersonaBarPage.bind(this)} />
                    }
                </PersonaBarPage>

                <PersonaBarPage isOpen={props.selectedPage === Pages.LanguageVerifier}>
                    <PersonaBarPageHeader title={resx.get("ResourceFileVerifier")}>
                    </PersonaBarPageHeader>
                    <LanguageVerifier portalId={state.portalId} closeLanguageVerifier={this.closePersonaBarPage.bind(this)} />
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
                        <TranslatePageContent cultureCode={state.cultureCode} closePersonaBarPage={this.closePersonaBarPage.bind(this)} portalId={state.portalId} />
                    }
                </PersonaBarPage>

            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    siteInfo: PropTypes.object,
    cultureCode: PropTypes.string
};


function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        siteInfo: state.siteInfo
    };
}


export default connect(mapStateToProps)(App);