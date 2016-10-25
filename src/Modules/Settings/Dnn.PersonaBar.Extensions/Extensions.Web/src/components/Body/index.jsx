import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    PaginationActions,
    VisiblePanelActions,
    ExtensionActions
} from "actions";
import SocialPanelBody from "dnn-social-panel-body";
import Localization from "localization";
import InstalledExtensions from "./InstalledExtensions";
import AvailableExtensions from "./AvailableExtensions";
import SocialPanelHeader from "dnn-social-panel-header";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import utilities from "utils";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import "./style.less";

const newExtension = {
    packageId: 67,
    packageType: "",
    name: "",
    friendlyName: "",
    description: "",
    version: "",
    inUse: "",
    upgradeUrl: "",
    packageIcon: "",
    license: "",
    owner: "",
    organization: "",
    url: "",
    email: ""
};

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {};
    }
    componentWillMount() {
        this.isHost = utilities.settings.isHost;
    }
    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
        this.setState({});
    }
    selectPanel(panel, event) {
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
    }

    onEditExtension(extensionBeingEditedIndex, packageId) {
        const { props } = this;
        props.dispatch(ExtensionActions.editExtension(packageId, extensionBeingEditedIndex,
            openEditPanel => {
                this.selectPanel(4);
            }
        ));
    }

    createExtension() {
        const { props } = this;
        props.dispatch(ExtensionActions.addExtension(validationMapExtensionBeingEdited(newExtension), openAddPanel => {
            this.selectPanel(2);
        }
        ));
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className="extension-body">
                <SocialPanelHeader title={Localization.get("ExtensionsLabel")}>
                    <Button type="primary" size="large" onClick={this.selectPanel.bind(this, 3)}>{Localization.get("ExtensionInstall.Action")}</Button>
                    {this.isHost && <Button type="secondary" size="large" onClick={this.createExtension.bind(this)}>{Localization.get("CreateExtension.Action")}</Button>}
                    {this.isHost && <Button type="secondary" size="large" onClick={this.selectPanel.bind(this, 1)}>{Localization.get("CreateModule.Action")}</Button>}
                </SocialPanelHeader>
                <SocialPanelBody>
                    <Tabs onSelect={this.handleSelect}
                        selectedIndex={props.tabIndex}
                        tabHeaders={[Localization.get("InstalledExtensions"), Localization.get("AvailableExtensions")]}>
                        <InstalledExtensions
                            isHost={this.isHost}
                            onEdit={this.onEditExtension.bind(this)}
                            onCancel={this.selectPanel.bind(this, 0)}
                            />
                        <AvailableExtensions />
                    </Tabs>
                </SocialPanelBody >

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
    return {
        installedPackages: state.extension.installedPackages,
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(Body);