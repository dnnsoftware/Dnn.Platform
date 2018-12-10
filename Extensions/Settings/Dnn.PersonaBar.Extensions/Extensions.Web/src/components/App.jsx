import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { InstallationActions } from "actions";
import Body from "./Body";
import PersonaBarPage from "dnn-persona-bar-page";
import NewExtensionModal from "./NewExtensionModal";
import NewModuleModal from "./NewModuleModal";
import InstallExtensionModal from "./installExtensionModal";
import EditExtension from "./EditExtension";
import CreatePackageModal from "./CreatePackageModal";
import DeleteExtension from "./DeleteExtension";
import { VisiblePanelActions } from "actions";
import util from "../utils";

class App extends Component {
    constructor() {
        super();
        this.state = {
            extensionBeingEdited: {}
        };
    }

    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        this.setState({
            referrer: "",
            referrerText: "",
            backToReferrerFunc: null
        }, () => {
            props.dispatch(VisiblePanelActions.selectPanel(panel));
        });        
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
        }, 750);
    }

    updateReferrerInfo(event) {
        this.setState({
            referrer: event.referrer,
            referrerText: event.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, event.backToReferrerFunc)
        });
    }

    UNSAFE_componentWillMount() {
        const { props } = this;

        document.addEventListener("installPortalTheme", (e) => {
            props.dispatch(InstallationActions.setIsPortalPackage(true, () => {
                this.selectPanel(3);
            }));
            this.updateReferrerInfo(e);
        }, false);

        if (util.settings.installPortalTheme) {
            props.dispatch(InstallationActions.setIsPortalPackage(true, () => {
                this.selectPanel(3);
            }));
            this.updateReferrerInfo(util.settings);
        }
    }

    /* End Extension CRUD methods */

    render() {
        const { props, state } = this;
        return (
            <div className="extensions-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0} className={(props.selectedPage !== 0 ? "hidden" : "")}>
                    <Body />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    {props.selectedPage === 1 && <NewModuleModal />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 2}>
                    {props.selectedPage === 2 && <NewExtensionModal onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 3}>
                    {props.selectedPage === 3 && <InstallExtensionModal
                        backToReferrer={state.referrer}
                        backToReferrerText={state.referrerText}
                        backToReferrerFunc={state.backToReferrerFunc}
                        onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 4}>
                    {props.selectedPage === 4 &&
                        <EditExtension />
                    }
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 5}>
                    {props.selectedPage === 5 &&
                        <CreatePackageModal onCancel={this.selectPanel.bind(this, 4)} />
                    }
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 6}>
                    {props.selectedPage === 6 &&
                        <DeleteExtension onCancel={this.selectPanel.bind(this, 0)} />
                    }
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    packageTypes: PropTypes.array,
    installedPackages: PropTypes.array
};


function mapStateToProps(state) {
    return {
        packageTypes: state.extension.packageTypes,
        installedPackages: state.extension.installedPackages,
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);