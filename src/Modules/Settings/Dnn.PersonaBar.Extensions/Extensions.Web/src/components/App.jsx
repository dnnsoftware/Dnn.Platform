import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import Body from "./Body";
import Modal from "dnn-modal";
import PersonaBarPage from "dnn-persona-bar-page";
import NewExtensionModal from "./NewExtensionModal";
import NewModuleModal from "./NewModuleModal";
import InstallExtensionModal from "./installExtensionModal";
import EditExtension from "./EditExtension";
import CreatePackageModal from "./CreatePackageModal";
import { VisiblePanelActions, ExtensionActions } from "actions";
import Localization from "localization";
import DropdownWithError from "dnn-dropdown-with-error";

const installExtensionModal = {
    key: "installExtension",
    header: "Install Extension"
};

const newExtensionModal = {
    key: "newExtension",
    header: "Create New Extension"
};

const newModuleModal = {
    key: "newModule",
    header: "Create New Module"
};

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
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
    }

    /* End Extension CRUD methods */

    render() {
        const {props, state} = this;
        return (
            <div className="extensions-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0} className={(props.selectedPage !== 0 ? "hidden" : "")}>
                    <Body />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    {props.selectedPage === 1 && <NewModuleModal onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 2}>
                    {props.selectedPage === 2 && <NewExtensionModal onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 3}>
                    {props.selectedPage === 3 && <InstallExtensionModal onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 4}>
                    {props.selectedPage === 4 &&
                        <EditExtension />
                    }
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 5}>
                    {props.selectedPage === 5 &&
                        <CreatePackageModal onCancel={this.selectPanel.bind(this, 4)}/>
                    }
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
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