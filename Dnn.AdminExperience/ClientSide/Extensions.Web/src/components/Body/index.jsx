import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DnnTabs as Tabs, PersonaBarPageBody, TextOverflowWrapper, PersonaBarPageHeader, GridCell, Button } from "@dnnsoftware/dnn-react-common";
import { PaginationActions, VisiblePanelActions, ExtensionActions, InstallationActions } from "actions";
import Localization from "localization";
import InstalledExtensions from "./InstalledExtensions";
import AvailableExtensions from "./AvailableExtensions";
import utilities from "utils";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import { ModuleCustomSettings, CoreLanguagePack, ExtensionLanguagePack } from "./packageCustomSettings";
import "./style.less";

const newExtension = {
    packageType: "",
    name: "",
    friendlyName: "",
    description: "",
    inUse: "",
    upgradeUrl: "",
    packageIcon: "",
    license: "",
    owner: "",
    organization: "",
    url: "",
    email: "",
    version: "0.0.0"
};

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {};
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        this.isHost = utilities.settings.isHost;
        if ((!props.locales || props.locales.length === 0)) {
            props.dispatch(ExtensionActions.getLocaleList());
        }
        if ((!props.localePackages || props.localePackages.length === 0)) {
            props.dispatch(ExtensionActions.getLocalePackageList());
        }
    }

    handleSelect(index/*, last*/) {
        const { props } = this;
        props.dispatch(PaginationActions.loadTab(index)); //index acts as scopeTypeId
        this.setState({});
    }
    
    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(InstallationActions.setIsPortalPackage(false, () => {
            props.dispatch(VisiblePanelActions.selectPanel(panel));
        }));
    }

    onEditExtension(extensionBeingEditedIndex, packageId) {
        const { props } = this;
        props.dispatch(ExtensionActions.editExtension(packageId, extensionBeingEditedIndex, () => {
            this.selectPanel(4);
        }));
    }

    createExtension() {
        const { props } = this;
        let _newExtension = Object.assign(newExtension, ModuleCustomSettings);
        _newExtension = Object.assign(newExtension, CoreLanguagePack);
        _newExtension = Object.assign(newExtension, ExtensionLanguagePack);
        _newExtension = Object.assign(newExtension, { locales: props.locales });
        _newExtension = Object.assign(newExtension, { packages: props.localePackages });
        props.dispatch(ExtensionActions.addExtension(validationMapExtensionBeingEdited(_newExtension), () => {
            this.selectPanel(2);
        }));
    }

    render() {
        const { props } = this;
        return (
            <GridCell className="extension-body">
                <PersonaBarPageHeader title={Localization.get("ExtensionsLabel")}>
                    {this.isHost && 
                        <Button type="primary" size="large" onClick={this.selectPanel.bind(this, 3)}>
                            <TextOverflowWrapper text={Localization.get("ExtensionInstall.Action") } maxWidth={120}/>
                        </Button>
                    }
                    {this.isHost && 
                        <Button type="secondary" size="large" onClick={this.createExtension.bind(this)}>
                            <TextOverflowWrapper text={Localization.get("CreateExtension.Action") } maxWidth={120}/>
                        </Button>
                    }
                    {this.isHost && 
                        <Button type="secondary" size="large" onClick={this.selectPanel.bind(this, 1)}>
                            <TextOverflowWrapper text={Localization.get("CreateModule.Action") } maxWidth={120}/>
                        </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    {this.isHost && <Tabs
                        onSelect={this.handleSelect}
                        selectedIndex={props.tabIndex}
                        tabHeaders={[Localization.get("InstalledExtensions"), Localization.get("AvailableExtensions")]}>
                        <InstalledExtensions
                            isHost={this.isHost}
                            onEdit={this.onEditExtension.bind(this)}
                            onCancel={this.selectPanel.bind(this, 0)} />
                        <AvailableExtensions />
                    </Tabs>}
                    {!this.isHost && <InstalledExtensions
                        isHost={this.isHost}
                        onEdit={this.onEditExtension.bind(this)}
                        onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPageBody>

            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    installedPackages: PropTypes.array,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return { installedPackages: state.extension.installedPackages, selectedInstalledPackageType: state.extension.selectedInstalledPackageType, tabIndex: state.pagination.tabIndex, locales: state.extension.locales, localePackages: state.extension.localePackages };
}

export default connect(mapStateToProps)(Body);
